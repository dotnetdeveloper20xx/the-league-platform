using TheLeague.Shared.Domain.Entities;

namespace TheLeague.Modules.Shop.Domain;

public class ProductCategory : TenantEntity
{
    public string Name { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; }

    private ProductCategory() { }

    public static ProductCategory Create(Guid clubId, string name, int displayOrder)
    {
        return new ProductCategory
        {
            ClubId = clubId,
            Name = name,
            DisplayOrder = displayOrder
        };
    }

    public void Update(string name, int displayOrder)
    {
        Name = name;
        DisplayOrder = displayOrder;
        UpdatedAt = DateTime.UtcNow;
    }
}
