using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Shop.Application.Dtos;
using TheLeague.Modules.Shop.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Shop.Application.Commands;

public record UpdateProductCommand(
    Guid ProductId,
    string Name,
    string? Description,
    decimal Price,
    bool IsActive,
    Guid? CategoryId
) : IRequest<Result<ProductDetailDto>>;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result<ProductDetailDto>>
{
    private readonly ShopDbContext _db;

    public UpdateProductCommandHandler(ShopDbContext db)
    {
        _db = db;
    }

    public async Task<Result<ProductDetailDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _db.Products
            .Include(p => p.Variants)
            .Include(p => p.Images)
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        if (product is null)
            return Result.Failure<ProductDetailDto>("Product not found");

        product.Update(request.Name, request.Description, request.Price, request.IsActive, request.CategoryId);
        await _db.SaveChangesAsync(cancellationToken);

        var dto = new ProductDetailDto(
            product.Id, product.Name, product.Description, product.Price,
            product.IsActive, product.CategoryId, product.Category?.Name,
            product.Variants.Select(v => new ProductVariantDto(v.Id, v.Size, v.Color, v.StockQuantity, v.Sku, v.IsActive)).ToList(),
            product.Images.Select(i => new ProductImageDto(i.Id, i.ImageUrl, i.DisplayOrder, i.IsMain)).ToList(),
            product.CreatedAt, product.UpdatedAt);

        return Result.Success(dto);
    }
}
