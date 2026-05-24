using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Sessions.Application.Dtos;
using TheLeague.Modules.Sessions.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Sessions.Application.Queries;

public record GetRecurringSchedulesQuery(
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<RecurringScheduleDto>>;

public class GetRecurringSchedulesQueryHandler : IRequestHandler<GetRecurringSchedulesQuery, PagedResult<RecurringScheduleDto>>
{
    private readonly SessionsDbContext _db;

    public GetRecurringSchedulesQueryHandler(SessionsDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<RecurringScheduleDto>> Handle(GetRecurringSchedulesQuery request, CancellationToken cancellationToken)
    {
        var query = _db.RecurringSchedules
            .AsNoTracking()
            .OrderBy(r => r.DayOfWeek)
            .ThenBy(r => r.StartTime);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(r => new RecurringScheduleDto(
                r.Id, r.Title, r.Category,
                r.VenueId, r.VenueName,
                r.DayOfWeek, r.StartTime, r.Duration,
                r.Capacity, r.Fee, r.HorizonWeeks,
                r.IsActive, r.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<RecurringScheduleDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
