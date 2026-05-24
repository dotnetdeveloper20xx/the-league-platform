using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Events.Application.Dtos;
using TheLeague.Modules.Events.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Enums;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Events.Application.Queries;

public record GetEventsQuery(
    EventType? EventType = null,
    EventStatus? Status = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<EventDto>>;

public class GetEventsQueryHandler : IRequestHandler<GetEventsQuery, PagedResult<EventDto>>
{
    private readonly EventsDbContext _db;

    public GetEventsQueryHandler(EventsDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<EventDto>> Handle(GetEventsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Events.AsNoTracking().AsQueryable();

        if (request.EventType.HasValue)
            query = query.Where(e => e.EventType == request.EventType.Value);

        if (request.Status.HasValue)
            query = query.Where(e => e.Status == request.Status.Value);

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
