using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Events.Application.Dtos;
using TheLeague.Modules.Events.Domain;
using TheLeague.Modules.Events.Infrastructure.Persistence;
using TheLeague.Shared.Contracts.Events;
using TheLeague.Shared.Contracts.Messaging;
using TheLeague.Shared.Domain.Enums;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Events.Application.Commands;

public record RegisterForEventCommand(
    Guid EventId,
    Guid MemberId,
    bool IsTicketPurchase,
    RSVPResponse? RsvpResponse = null,
    int GuestCount = 0
) : IRequest<Result<EventRegistrationDto>>;

public class RegisterForEventCommandHandler : IRequestHandler<RegisterForEventCommand, Result<EventRegistrationDto>>
{
    private readonly EventsDbContext _db;
    private readonly IIntegrationEventBus _eventBus;

    public RegisterForEventCommandHandler(EventsDbContext db, IIntegrationEventBus eventBus)
    {
        _db = db;
        _eventBus = eventBus;
    }

    public async Task<Result<EventRegistrationDto>> Handle(RegisterForEventCommand request, CancellationToken cancellationToken)
    {
        var evt = await _db.Events.FirstOrDefaultAsync(e => e.Id == request.EventId, cancellationToken);
        if (evt is null)
            return Result.Failure<EventRegistrationDto>("Event not found.");

        if (evt.Status != EventStatus.RegistrationOpen)
            return Result.Failure<EventRegistrationDto>("Event registration is not open.");

        if (evt.IsAtCapacity())
            return Result.Failure<EventRegistrationDto>("Event is at full capacity.");

        // Check for existing active registration
        var existingReg = await _db.EventRegistrations
            .FirstOrDefaultAsync(r => r.EventId == request.EventId && r.MemberId == request.MemberId && r.CancelledAt == null, cancellationToken);
        if (existingReg is not null)
            return Result.Failure<EventRegistrationDto>("Member is already registered for this event.");

        string registrationType;

        if (request.IsTicketPurchase && evt.IsTicketed)
        {
            registrationType = "Ticket";

            // Generate ticket
            var ticketNumber = $"TKT-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
            var qrCodeData = $"EVT:{evt.Id}|TKT:{ticketNumber}|MBR:{request.MemberId}";
            var pricePaid = evt.MemberPrice ?? evt.StandardPrice ?? 0;

            var ticket = EventTicket.Create(
                evt.ClubId, evt.Id, request.MemberId,
                ticketNumber, qrCodeData, pricePaid);

            _db.EventTickets.Add(ticket);

            await _eventBus.PublishAsync(
                new TicketPurchasedEvent(ticket.Id, evt.Id, request.MemberId, evt.ClubId),
                cancellationToken);
        }
        else if (!request.IsTicketPurchase && evt.AllowRsvp)
        {
            registrationType = "RSVP";

            var rsvpResponse = request.RsvpResponse ?? RSVPResponse.Attending;

            // Check for existing RSVP and update or create
            var existingRsvp = await _db.EventRSVPs
                .FirstOrDefaultAsync(r => r.EventId == request.EventId && r.MemberId == request.MemberId, cancellationToken);

            if (existingRsvp is not null)
            {
                existingRsvp.UpdateResponse(rsvpResponse, request.GuestCount);
            }
            else
            {
                var rsvp = EventRSVP.Create(evt.ClubId, evt.Id, request.MemberId, rsvpResponse, request.GuestCount);
                _db.EventRSVPs.Add(rsvp);
            }
        }
        else
        {
            return Result.Failure<EventRegistrationDto>("Invalid registration type for this event.");
        }

        var registration = EventRegistration.Create(evt.ClubId, evt.Id, request.MemberId, registrationType);
        _db.EventRegistrations.Add(registration);

        evt.IncrementRegistrationCount();
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(new EventRegistrationDto(
            registration.Id, registration.EventId, registration.MemberId,
            registration.RegistrationType, registration.RegisteredAt,
            registration.CancelledAt, registration.RefundInitiated));
    }
}
