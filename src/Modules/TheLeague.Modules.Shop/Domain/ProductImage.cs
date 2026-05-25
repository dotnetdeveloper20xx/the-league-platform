using TheLeague.Shared.Domain.Entities;

namespace TheLeague.Modules.Shop.Domain;

public class ProductImage : TenantEntity
{
    public Guid ProductId { get; private set; }
    public string ImageUrl { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; }
    public bool IsMain { get; private set; }

    // Navigation
    public Product? Product { get; private set; }

    private ProductImage() { }

    public static ProductImage Create(Guid clubId, Guid productId, string imageUrl, int displayOrder, bool isMain)
    {
        return new ProductImage
        {
            ClubId = clubId,
            ProductId = productId,
            ImageUrl = imageUrl,
            DisplayOrder = displayOrder,
            IsMain = isMain
        };
    }
}
