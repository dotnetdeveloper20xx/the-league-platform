using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Shop.Application.Dtos;
using TheLeague.Modules.Shop.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Shop.Application.Queries;

public record GetProductsQuery(
    Guid? CategoryId = null,
    string? SortBy = null,
    bool SortDescending = false,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<ProductListDto>>;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PagedResult<ProductListDto>>
{
    private readonly ShopDbContext _db;

    public GetProductsQueryHandler(ShopDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<ProductListDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var pageSize = Math.Min(request.PageSize, 100);
        if (pageSize < 1) pageSize = 20;

        var query = _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Variants)
            .AsQueryable();

        if (request.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);
        }

        // Sort
        query = request.SortBy?.ToLower() switch
        {
            "price" => request.SortDescending
                ? query.OrderByDescending(p => p.Price)
                : query.OrderBy(p => p.Price),
            "name" => request.SortDescending
                ? query.OrderByDescending(p => p.Name)
                : query.OrderBy(p => p.Name),
            _ => query.OrderBy(p => p.Name)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.Page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductListDto(
                p.Id, p.Name, p.Description, p.Price, p.IsActive,
                p.CategoryId, p.Category != null ? p.Category.Name : null,
                p.Variants.Count, p.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<ProductListDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = pageSize
        };
    }
}
