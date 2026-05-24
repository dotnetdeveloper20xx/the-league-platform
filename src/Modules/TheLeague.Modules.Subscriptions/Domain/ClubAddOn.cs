using TheLeague.Shared.Domain.Entities;

namespace TheLeague.Modules.Subscriptions.Domain;

public class ClubAddOn : BaseEntity
{
    public Guid ClubId { get; private set; }
    public Guid AddOnId { get; private set; }
    public DateTime PurchasedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; private set; }
    public bool IsActive { get; private set; } = true;
}
