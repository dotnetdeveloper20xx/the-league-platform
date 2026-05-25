using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Facilities.Application.Dtos;
using TheLeague.Modules.Facilities.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Facilities.Application.Queries;

public record GetFacilityBookingsQuery(
    Guid? FacilityId = null,
    DateOnly? FromDate = null,
    DateOnly? ToDate = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<FacilityBookingDto>>;

public class GetFacilityBookingsQueryHandler : IRequestHandler<GetFacilityBookingsQuery, PagedResult<FacilityBookingDto>>
{
    private readonly FacilitiesDbContext _db;

    public GetFacilityBookingsQueryHandler(FacilitiesDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<FacilityBookingDto>> Handle(GetFacilityBookingsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.FacilityBookings.AsNoTracking().AsQueryable();

        if (request.FacilityId.HasValue)
            query = query.Where(b => b.FacilityId == request.FacilityId.Value);

        if (request.FromDate.HasValue)
            query = query.Where(b => b.BookingDate >= request.FromDate.Value);

        if (request.ToDate.HasValue)
            query = query.Where(b => b.BookingDate <= request.ToDate.Value);

        query = query.OrderByDescending(b => b.BookingDate).ThenBy(b => b.StartTime);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(b => new FacilityBookingDto(
                b.Id, b.FacilityId, b.MemberId, b.BookingDate,
                b.StartTime, b.EndTime, b.Duration, b.IsMember,
                b.PricePaid, b.Status, b.BookingReference, b.BookedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<FacilityBookingDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
