using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Events.Application.Dtos;
using TheLeague.Modules.Events.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Events.Application.Queries;

public record GetEventByIdQuery(Guid EventId) : IRequest<Result<EventDetailDto>>;

public class GetEventByIdQueryHandler : IRequestHandler<GetEventByIdQuery, Result<EventDetailDto>>
{
    private readonly EventsDbContext _db;

    public GetEventByIdQueryHandler(EventsDbContext db)
    {
        _db = db;
    }

    public async Task<Result<EventDetailDto>> Handle(GetEventByIdQuery request, CancellationToken cancellationToken)
    {
        var evt = await _db.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == request.EventId, cancellationToken);

        if (evt is null)
            return Result.Failure<EventDetailDto>("Event not found.");

        var registrations = await _db.EventRegistrations
            .AsNoTracking()
            .Where(r => r.EventId == request.EventId)
            .Select(r => new EventRegistrationDto(
                r.Id, r.EventId, r.MemberId, r.RegistrationType,
                r.RegisteredAt, r.CancelledAt, r.RefundInitiated))
            .ToListAsync(cancellationToken);

        var detail = new EventDetailDto(
            evt.Id, evt.ClubId, evt.Title, evt.Description, evt.EventType, evt.Status,
            evt.StartDateTime, evt.EndDateTime, evt.VenueId, evt.VenueName,
            evt.Capacity, evt.CurrentRegistrationCount, evt.IsTicketed,
            evt.StandardPrice, evt.MemberPrice, evt.AllowRsvp,
            evt.CancellationDeadlineHours, registrations,
            evt.CreatedAt, evt.UpdatedAt);

        return Result.Success(detail);
    }
}
