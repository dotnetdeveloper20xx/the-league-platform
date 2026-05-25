using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Equipment.Application.Dtos;
using TheLeague.Modules.Equipment.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Enums;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Equipment.Application.Queries;

public record GetEquipmentQuery(
    EquipmentCategory? Category = null,
    EquipmentCondition? Condition = null,
    string? SearchTerm = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<EquipmentDto>>;

public class GetEquipmentQueryHandler : IRequestHandler<GetEquipmentQuery, PagedResult<EquipmentDto>>
{
    private readonly EquipmentDbContext _db;

    public GetEquipmentQueryHandler(EquipmentDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<EquipmentDto>> Handle(GetEquipmentQuery request, CancellationToken cancellationToken)
    {
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = _db.Equipment.AsNoTracking().AsQueryable();

        if (request.Category.HasValue)
            query = query.Where(e => e.Category == request.Category.Value);

        if (request.Condition.HasValue)
            query = query.Where(e => e.Condition == request.Condition.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(e =>
                e.Name.ToLower().Contains(term) ||
                e.Location.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(e => e.Name)
            .Skip((request.Page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new EquipmentDto(
                e.Id, e.Name, e.Category, e.Condition, e.Location,
                e.PurchaseDate, e.Value, e.AnnualDepreciationRate,
                e.SerialNumber, e.IsActive, e.CreatedAt, e.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<EquipmentDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = pageSize
        };
    }
}
