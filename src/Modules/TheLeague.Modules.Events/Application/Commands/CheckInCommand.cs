using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Events.Application.Dtos;
using TheLeague.Modules.Events.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Events.Application.Commands;

public record CheckInCommand(Guid EventId, string TicketNumber) : IRequest<Result<EventTicketDto>>;

public class CheckInCommandHandler : IRequestHandler<CheckInCommand, Result<EventTicketDto>>
{
    private readonly EventsDbContext _db;

    public CheckInCommandHandler(EventsDbContext db)
    {
        _db = db;
    }

    public async Task<Result<EventTicketDto>> Handle(CheckInCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _db.EventTickets
            .FirstOrDefaultAsync(t => t.EventId == request.EventId && t.TicketNumber == request.TicketNumber, cancellationToken);

        if (ticket is null)
            return Result.Failure<EventTicketDto>("Ticket not found for this event.");

        ticket.CheckIn();
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(new EventTicketDto(
            ticket.Id, ticket.EventId, ticket.MemberId,
            ticket.TicketNumber, ticket.QRCodeData, ticket.PricePaid,
            ticket.PurchasedAt, ticket.IsCheckedIn, ticket.CheckedInAt));
    }
}
