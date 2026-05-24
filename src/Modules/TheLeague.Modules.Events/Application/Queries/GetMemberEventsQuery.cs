using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Events.Application.Dtos;
using TheLeague.Modules.Events.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Events.Application.Queries;

public record GetMemberEventsQuery(Guid MemberId, int Page = 1, int PageSize = 20) : IRequest<PagedResult<EventDto>>;

public class GetMemberEventsQueryHandler : IRequestHandler<GetMemberEventsQuery, PagedResult<EventDto>>
{
    private readonly EventsDbContext _db;

    public GetMemberEventsQueryHandler(EventsDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<EventDto>> Handle(GetMemberEventsQuery request, CancellationToken cancellationToken)
    {
        var registeredEventIds = await _db.EventRegistrations
            .AsNoTracking()
            .Where(r => r.MemberId == request.MemberId && r.CancelledAt == null)
            .Select(r => r.EventId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var query = _db.Events
            .AsNoTracking()
            .Where(e => registeredEventIds.Contains(e.Id));

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(e => e.StartDateTime)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(e => new EventDto(
                e.Id, e.ClubId, e.Title, e.Description, e.EventType, e.Status,
                e.StartDateTime, e.EndDateTime, e.VenueId, e.VenueName,
                e.Capacity, e.CurrentRegistrationCount, e.IsTicketed,
                e.StandardPrice, e.MemberPrice, e.AllowRsvp,
                e.CancellationDeadlineHours, e.CreatedAt, e.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<EventDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
