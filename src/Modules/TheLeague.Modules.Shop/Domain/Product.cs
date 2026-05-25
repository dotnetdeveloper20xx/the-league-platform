using TheLeague.Shared.Domain.Entities;

namespace TheLeague.Modules.Shop.Domain;

public class Product : TenantEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public bool IsActive { get; private set; } = true;
    public Guid? CategoryId { get; private set; }

    // Navigation properties
    public ProductCategory? Category { get; private set; }
    public ICollection<ProductVariant> Variants { get; private set; } = new List<ProductVariant>();
    public ICollection<ProductImage> Images { get; private set; } = new List<ProductImage>();

    public const int MaxImages = 8;

    private Product() { }

    public static Product Create(Guid clubId, string name, string? description, decimal price, Guid? categoryId)
    {
        if (price < 0.01m || price > 999999.99m)
            throw new ArgumentOutOfRangeException(nameof(price), "Price must be between 0.01 and 999999.99");

        return new Product
        {
            ClubId = clubId,
            Name = name,
            Description = description,
            Price = price,
            CategoryId = categoryId
        };
    }

    public void Update(string name, string? description, decimal price, bool isActive, Guid? categoryId)
    {
        if (price < 0.01m || price > 999999.99m)
            throw new ArgumentOutOfRangeException(nameof(price), "Price must be between 0.01 and 999999.99");

        Name = name;
        Description = description;
        Price = price;
        IsActive = isActive;
        CategoryId = categoryId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
