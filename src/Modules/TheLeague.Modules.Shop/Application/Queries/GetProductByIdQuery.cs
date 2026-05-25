using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Shop.Application.Dtos;
using TheLeague.Modules.Shop.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Shop.Application.Queries;

public record GetProductByIdQuery(Guid ProductId) : IRequest<Result<ProductDetailDto>>;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Result<ProductDetailDto>>
{
    private readonly ShopDbContext _db;

    public GetProductByIdQueryHandler(ShopDbContext db)
    {
        _db = db;
    }

    public async Task<Result<ProductDetailDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Variants)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        if (product is null)
            return Result.Failure<ProductDetailDto>("Product not found");

        var dto = new ProductDetailDto(
            product.Id, product.Name, product.Description, product.Price,
            product.IsActive, product.CategoryId, product.Category?.Name,
            product.Variants.Select(v => new ProductVariantDto(v.Id, v.Size, v.Color, v.StockQuantity, v.Sku, v.IsActive)).ToList(),
            product.Images.OrderBy(i => i.DisplayOrder).Select(i => new ProductImageDto(i.Id, i.ImageUrl, i.DisplayOrder, i.IsMain)).ToList(),
            product.CreatedAt, product.UpdatedAt);

        return Result.Success(dto);
    }
}
