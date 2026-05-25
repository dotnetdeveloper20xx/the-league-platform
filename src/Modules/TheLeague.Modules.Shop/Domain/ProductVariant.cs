using TheLeague.Shared.Domain.Entities;

namespace TheLeague.Modules.Shop.Domain;

public class ProductVariant : TenantEntity
{
    public Guid ProductId { get; private set; }
    public string? Size { get; private set; }
    public string? Color { get; private set; }
    public int StockQuantity { get; private set; }
    public string? Sku { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Navigation
    public Product? Product { get; private set; }

    private ProductVariant() { }

    public static ProductVariant Create(Guid clubId, Guid productId, string? size, string? color, int stockQuantity, string? sku)
    {
        if (stockQuantity < 0 || stockQuantity > 99999)
            throw new ArgumentOutOfRangeException(nameof(stockQuantity), "Stock quantity must be between 0 and 99999");

        return new ProductVariant
        {
            ClubId = clubId,
            ProductId = productId,
            Size = size,
            Color = color,
            StockQuantity = stockQuantity,
            Sku = sku
        };
    }

    public void DecrementStock(int quantity = 1)
    {
        if (StockQuantity < quantity)
            throw new InvalidOperationException("Insufficient stock");

        StockQuantity -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReplenishStock(int quantity)
    {
        if (quantity < 1)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be at least 1");

        StockQuantity += quantity;
        if (StockQuantity > 99999)
            StockQuantity = 99999;

        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
