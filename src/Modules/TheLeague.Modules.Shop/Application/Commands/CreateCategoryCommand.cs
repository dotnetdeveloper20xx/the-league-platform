using MediatR;
using TheLeague.Modules.Shop.Application.Dtos;
using TheLeague.Modules.Shop.Domain;
using TheLeague.Modules.Shop.Infrastructure.Persistence;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Shop.Application.Commands;

public record CreateCategoryCommand(
    string Name,
    int DisplayOrder
) : IRequest<Result<ProductCategoryDto>>;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Result<ProductCategoryDto>>
{
    private readonly ShopDbContext _db;
    private readonly ITenantService _tenantService;

    public CreateCategoryCommandHandler(ShopDbContext db, ITenantService tenantService)
    {
        _db = db;
        _tenantService = tenantService;
    }

    public async Task<Result<ProductCategoryDto>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var clubId = _tenantService.CurrentTenantId!.Value;

        var category = ProductCategory.Create(clubId, request.Name, request.DisplayOrder);

        _db.ProductCategories.Add(category);
        await _db.SaveChangesAsync(cancellationToken);

        var dto = new ProductCategoryDto(category.Id, category.Name, category.DisplayOrder);
        return Result.Success(dto);
    }
}
