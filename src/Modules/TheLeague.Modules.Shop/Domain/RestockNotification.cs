using TheLeague.Shared.Domain.Entities;

namespace TheLeague.Modules.Shop.Domain;

public class RestockNotification : TenantEntity
{
    public Guid ProductVariantId { get; private set; }
    public Guid MemberId { get; private set; }
    public DateTime? NotifiedAt { get; private set; }

    private RestockNotification() { }

    public static RestockNotification Create(Guid clubId, Guid productVariantId, Guid memberId)
    {
        return new RestockNotification
        {
            ClubId = clubId,
            ProductVariantId = productVariantId,
            MemberId = memberId
        };
    }

    public void MarkNotified()
    {
        NotifiedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
