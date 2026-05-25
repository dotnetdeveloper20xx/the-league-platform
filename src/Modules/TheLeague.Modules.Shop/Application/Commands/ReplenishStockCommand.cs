using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Shop.Application.Dtos;
using TheLeague.Modules.Shop.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Shop.Application.Commands;

public record ReplenishStockCommand(
    Guid VariantId,
    int Quantity
) : IRequest<Result<ProductVariantDto>>;

public class ReplenishStockCommandHandler : IRequestHandler<ReplenishStockCommand, Result<ProductVariantDto>>
{
    private readonly ShopDbContext _db;

    public ReplenishStockCommandHandler(ShopDbContext db)
    {
        _db = db;
    }

    public async Task<Result<ProductVariantDto>> Handle(ReplenishStockCommand request, CancellationToken cancellationToken)
    {
        var variant = await _db.ProductVariants
            .FirstOrDefaultAsync(v => v.Id == request.VariantId, cancellationToken);

        if (variant is null)
            return Result.Failure<ProductVariantDto>("Product variant not found");

        variant.ReplenishStock(request.Quantity);
        await _db.SaveChangesAsync(cancellationToken);

        // Notify interested members
        var notifications = await _db.RestockNotifications
            .Where(n => n.ProductVariantId == request.VariantId && n.NotifiedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var notification in notifications)
        {
            notification.MarkNotified();
            // TODO: Send notification to member via communications module
        }

        await _db.SaveChangesAsync(cancellationToken);

        var dto = new ProductVariantDto(variant.Id, variant.Size, variant.Color, variant.StockQuantity, variant.Sku, variant.IsActive);
        return Result.Success(dto);
    }
}
