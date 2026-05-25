using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Shop.Application.Dtos;
using TheLeague.Modules.Shop.Domain;
using TheLeague.Modules.Shop.Infrastructure.Persistence;
using TheLeague.Shared.Contracts.Events;
using TheLeague.Shared.Contracts.Messaging;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Shop.Application.Commands;

public record PurchaseProductCommand(
    Guid MemberId,
    List<PurchaseItem> Items
) : IRequest<Result<OrderDto>>;

public record PurchaseItem(
    Guid ProductId,
    Guid? VariantId,
    int Quantity
);

public class PurchaseProductCommandHandler : IRequestHandler<PurchaseProductCommand, Result<OrderDto>>
{
    private readonly ShopDbContext _db;
    private readonly ITenantService _tenantService;
    private readonly IIntegrationEventBus _eventBus;

    public PurchaseProductCommandHandler(
        ShopDbContext db,
        ITenantService tenantService,
        IIntegrationEventBus eventBus)
    {
        _db = db;
        _tenantService = tenantService;
        _eventBus = eventBus;
    }

    public async Task<Result<OrderDto>> Handle(PurchaseProductCommand request, CancellationToken cancellationToken)
    {
        var clubId = _tenantService.CurrentTenantId!.Value;

        if (request.Items == null || request.Items.Count == 0)
            return Result.Failure<OrderDto>("At least one item is required");

        var orderItems = new List<OrderItem>();
        decimal totalAmount = 0;

        foreach (var item in request.Items)
        {
            var product = await _db.Products
                .FirstOrDefaultAsync(p => p.Id == item.ProductId, cancellationToken);

            if (product is null)
                return Result.Failure<OrderDto>($"Product {item.ProductId} not found");

            if (!product.IsActive)
                return Result.Failure<OrderDto>($"Product '{product.Name}' is not available");

            string? variantDescription = null;

            if (item.VariantId.HasValue)
            {
                var variant = await _db.ProductVariants
                    .FirstOrDefaultAsync(v => v.Id == item.VariantId.Value && v.ProductId == item.ProductId, cancellationToken);

                if (variant is null)
                    return Result.Failure<OrderDto>($"Variant {item.VariantId} not found for product '{product.Name}'");

                if (!variant.IsActive)
                    return Result.Failure<OrderDto>($"Variant is not available for product '{product.Name}'");

                if (variant.StockQuantity < item.Quantity)
                    return Result.Failure<OrderDto>($"Insufficient stock for product '{product.Name}'. Available: {variant.StockQuantity}");

                variant.DecrementStock(item.Quantity);

                variantDescription = $"{variant.Size ?? ""} {variant.Color ?? ""}".Trim();
                if (string.IsNullOrEmpty(variantDescription))
                    variantDescription = variant.Sku;
            }

            var lineTotal = product.Price * item.Quantity;
            totalAmount += lineTotal;

            // We'll create order items after the order is created
            orderItems.Add(OrderItem.Create(
                clubId, Guid.Empty, item.ProductId, item.VariantId,
                product.Name, variantDescription, item.Quantity, product.Price));
        }

        // Generate order reference
        var orderReference = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

        var order = Order.Create(clubId, request.MemberId, orderReference, totalAmount);
        _db.Orders.Add(order);
        await _db.SaveChangesAsync(cancellationToken);

        // Now create order items with the correct OrderId
        foreach (var item in orderItems)
        {
            var orderItem = OrderItem.Create(
                clubId, order.Id, item.ProductId, item.VariantId,
                item.ProductName, item.VariantDescription, item.Quantity, item.UnitPrice);
            _db.OrderItems.Add(orderItem);
        }

        await _db.SaveChangesAsync(cancellationToken);

        // Publish integration event
        await _eventBus.PublishAsync(
            new OrderCreatedEvent(order.Id, request.MemberId, clubId),
            cancellationToken);

        // Reload with items
        var savedOrder = await _db.Orders
            .Include(o => o.Items)
            .FirstAsync(o => o.Id == order.Id, cancellationToken);

        return Result.Success(MapToDto(savedOrder));
    }

    private static OrderDto MapToDto(Order order) => new(
        order.Id, order.MemberId, order.OrderReference, order.Status,
        order.TotalAmount, order.OrderedAt, order.ConfirmedAt,
        order.DispatchedAt, order.DeliveredAt, order.RefundedAt,
        order.Items.Select(i => new OrderItemDto(
            i.Id, i.ProductId, i.VariantId, i.ProductName,
            i.VariantDescription, i.Quantity, i.UnitPrice, i.TotalPrice)).ToList());
}
