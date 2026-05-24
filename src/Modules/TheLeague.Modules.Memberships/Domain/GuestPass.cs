using TheLeague.Shared.Domain.Entities;

namespace TheLeague.Modules.Memberships.Domain;

public class GuestPass : TenantEntity
{
    public Guid? MemberId { get; private set; }
    public string PassCode { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public bool IsUsed { get; private set; }
    public DateTime? UsedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }

    public static GuestPass Create(Guid clubId, Guid? memberId, string passCode, decimal price, DateTime expiresAt)
    {
        return new GuestPass
        {
            ClubId = clubId,
            MemberId = memberId,
            PassCode = passCode,
            Price = price,
            ExpiresAt = expiresAt,
            IsUsed = false
        };
    }

    public void MarkUsed()
    {
        IsUsed = true;
        UsedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
