using TheLeague.Shared.Domain.Entities;

namespace TheLeague.Modules.Shop.Domain;

public class OrderItem : TenantEntity
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid? VariantId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public string? VariantDescription { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TotalPrice { get; private set; }

    // Navigation
    public Order? Order { get; private set; }

    private OrderItem() { }

    public static OrderItem Create(
        Guid clubId, Guid orderId, Guid productId, Guid? variantId,
        string productName, string? variantDescription, int quantity, decimal unitPrice)
    {
        return new OrderItem
        {
            ClubId = clubId,
            OrderId = orderId,
            ProductId = productId,
            VariantId = variantId,
            ProductName = productName,
            VariantDescription = variantDescription,
            Quantity = quantity,
            UnitPrice = unitPrice,
            TotalPrice = quantity * unitPrice
        };
    }
}
