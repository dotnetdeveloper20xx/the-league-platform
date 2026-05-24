using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Sessions.Application.Dtos;
using TheLeague.Modules.Sessions.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Sessions.Application.Queries;

public record GetMemberBookingsQuery(
    Guid MemberId,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<SessionBookingDto>>;

public class GetMemberBookingsQueryHandler : IRequestHandler<GetMemberBookingsQuery, PagedResult<SessionBookingDto>>
{
    private readonly SessionsDbContext _db;

    public GetMemberBookingsQueryHandler(SessionsDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<SessionBookingDto>> Handle(GetMemberBookingsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.SessionBookings
            .AsNoTracking()
            .Where(b => b.MemberId == request.MemberId)
            .OrderByDescending(b => b.BookedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(b => new SessionBookingDto(
                b.Id, b.SessionId, b.MemberId, b.Status, b.BookedAt, b.CancelledAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<SessionBookingDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
