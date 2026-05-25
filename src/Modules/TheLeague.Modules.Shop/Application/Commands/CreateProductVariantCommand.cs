using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Shop.Application.Dtos;
using TheLeague.Modules.Shop.Domain;
using TheLeague.Modules.Shop.Infrastructure.Persistence;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Shop.Application.Commands;

public record CreateProductVariantCommand(
    Guid ProductId,
    string? Size,
    string? Color,
    int StockQuantity,
    string? Sku
) : IRequest<Result<ProductVariantDto>>;

public class CreateProductVariantCommandHandler : IRequestHandler<CreateProductVariantCommand, Result<ProductVariantDto>>
{
    private readonly ShopDbContext _db;
    private readonly ITenantService _tenantService;

    public CreateProductVariantCommandHandler(ShopDbContext db, ITenantService tenantService)
    {
        _db = db;
        _tenantService = tenantService;
    }

    public async Task<Result<ProductVariantDto>> Handle(CreateProductVariantCommand request, CancellationToken cancellationToken)
    {
        var clubId = _tenantService.CurrentTenantId!.Value;

        var productExists = await _db.Products.AnyAsync(p => p.Id == request.ProductId, cancellationToken);
        if (!productExists)
            return Result.Failure<ProductVariantDto>("Product not found");

        var variant = ProductVariant.Create(
            clubId, request.ProductId, request.Size, request.Color, request.StockQuantity, request.Sku);

        _db.ProductVariants.Add(variant);
        await _db.SaveChangesAsync(cancellationToken);

        var dto = new ProductVariantDto(variant.Id, variant.Size, variant.Color, variant.StockQuantity, variant.Sku, variant.IsActive);
        return Result.Success(dto);
    }
}
