using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Events.Infrastructure.Persistence;
using TheLeague.Shared.Contracts.Events;
using TheLeague.Shared.Contracts.Messaging;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Events.Application.Commands;

public record CancelEventCommand(Guid EventId) : IRequest<Result>;

public class CancelEventCommandHandler : IRequestHandler<CancelEventCommand, Result>
{
    private readonly EventsDbContext _db;
    private readonly IIntegrationEventBus _eventBus;

    public CancelEventCommandHandler(EventsDbContext db, IIntegrationEventBus eventBus)
    {
        _db = db;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(CancelEventCommand request, CancellationToken cancellationToken)
    {
        var evt = await _db.Events.FirstOrDefaultAsync(e => e.Id == request.EventId, cancellationToken);
        if (evt is null)
            return Result.Failure("Event not found.");

        evt.Cancel();

        // Initiate refunds for all active ticketed registrations
        var activeRegistrations = await _db.EventRegistrations
            .Where(r => r.EventId == request.EventId && r.CancelledAt == null)
            .ToListAsync(cancellationToken);

        foreach (var registration in activeRegistrations)
        {
            registration.Cancel();
            if (registration.RegistrationType == "Ticket")
            {
                registration.InitiateRefund();
            }
        }

        await _db.SaveChangesAsync(cancellationToken);

        // Publish event cancelled integration event (triggers notifications to attendees)
        await _eventBus.PublishAsync(
            new EventCancelledEvent(evt.Id, evt.ClubId),
            cancellationToken);

        return Result.Success("Event cancelled. All attendees will be notified and refunds initiated for ticketed registrations.");
    }
}
