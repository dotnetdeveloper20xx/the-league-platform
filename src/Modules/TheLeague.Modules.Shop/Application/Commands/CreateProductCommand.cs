using MediatR;
using TheLeague.Modules.Shop.Application.Dtos;
using TheLeague.Modules.Shop.Domain;
using TheLeague.Modules.Shop.Infrastructure.Persistence;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Shop.Application.Commands;

public record CreateProductCommand(
    string Name,
    string? Description,
    decimal Price,
    Guid? CategoryId
) : IRequest<Result<ProductDetailDto>>;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<ProductDetailDto>>
{
    private readonly ShopDbContext _db;
    private readonly ITenantService _tenantService;

    public CreateProductCommandHandler(ShopDbContext db, ITenantService tenantService)
    {
        _db = db;
        _tenantService = tenantService;
    }

    public async Task<Result<ProductDetailDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var clubId = _tenantService.CurrentTenantId!.Value;

        var product = Product.Create(clubId, request.Name, request.Description, request.Price, request.CategoryId);

        _db.Products.Add(product);
        await _db.SaveChangesAsync(cancellationToken);

        var dto = new ProductDetailDto(
            product.Id, product.Name, product.Description, product.Price,
            product.IsActive, product.CategoryId, null,
            new List<ProductVariantDto>(), new List<ProductImageDto>(),
            product.CreatedAt, product.UpdatedAt);

        return Result.Success(dto);
    }
}
