using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Sessions.Application.Dtos;
using TheLeague.Modules.Sessions.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Sessions.Application.Queries;

public record GetSessionsQuery(
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<SessionDto>>;

public class GetSessionsQueryHandler : IRequestHandler<GetSessionsQuery, PagedResult<SessionDto>>
{
    private readonly SessionsDbContext _db;

    public GetSessionsQueryHandler(SessionsDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<SessionDto>> Handle(GetSessionsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Sessions.AsNoTracking().AsQueryable();

        if (request.FromDate.HasValue)
            query = query.Where(s => s.StartTime >= request.FromDate.Value);

        if (request.ToDate.HasValue)
            query = query.Where(s => s.StartTime <= request.ToDate.Value);

        query = query.OrderBy(s => s.StartTime);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new SessionDto(
                s.Id, s.Title, s.Category,
                s.VenueId, s.VenueName,
                s.StartTime, s.EndTime, s.Duration,
                s.Capacity, s.Fee, s.CurrentBookingCount,
                s.Capacity - s.CurrentBookingCount,
                s.IsCancelled, s.CancellationReason,
                s.CancellationDeadlineHours, s.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<SessionDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
