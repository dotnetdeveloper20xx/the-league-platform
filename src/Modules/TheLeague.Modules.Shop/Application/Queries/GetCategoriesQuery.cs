using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Shop.Application.Dtos;
using TheLeague.Modules.Shop.Infrastructure.Persistence;

namespace TheLeague.Modules.Shop.Application.Queries;

public record GetCategoriesQuery : IRequest<List<ProductCategoryDto>>;

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, List<ProductCategoryDto>>
{
    private readonly ShopDbContext _db;

    public GetCategoriesQueryHandler(ShopDbContext db)
    {
        _db = db;
    }

    public async Task<List<ProductCategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        return await _db.ProductCategories
            .AsNoTracking()
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new ProductCategoryDto(c.Id, c.Name, c.DisplayOrder))
            .ToListAsync(cancellationToken);
    }
}
