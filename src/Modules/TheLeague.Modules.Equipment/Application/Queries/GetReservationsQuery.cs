using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Equipment.Application.Dtos;
using TheLeague.Modules.Equipment.Domain;
using TheLeague.Modules.Equipment.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Equipment.Application.Queries;

public record GetReservationsQuery(
    Guid? EquipmentId = null,
    ReservationStatus? Status = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<ReservationDto>>;

public class GetReservationsQueryHandler : IRequestHandler<GetReservationsQuery, PagedResult<ReservationDto>>
{
    private readonly EquipmentDbContext _db;

    public GetReservationsQueryHandler(EquipmentDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<ReservationDto>> Handle(GetReservationsQuery request, CancellationToken cancellationToken)
    {
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = _db.EquipmentReservations.AsNoTracking().AsQueryable();

        if (request.EquipmentId.HasValue)
            query = query.Where(r => r.EquipmentId == request.EquipmentId.Value);

        if (request.Status.HasValue)
            query = query.Where(r => r.Status == request.Status.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(r => r.StartDate)
            .Skip((request.Page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ReservationDto(
                r.Id, r.EquipmentId, r.MemberId,
                r.StartDate, r.EndDate, r.Status,
                r.Notes, r.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<ReservationDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = pageSize
        };
    }
}
