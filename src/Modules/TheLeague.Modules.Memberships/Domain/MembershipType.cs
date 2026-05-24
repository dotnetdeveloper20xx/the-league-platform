using TheLeague.Shared.Domain.Entities;
using TheLeague.Shared.Domain.Enums;

namespace TheLeague.Modules.Memberships.Domain;

public class MembershipType : TenantEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public BillingCycle BillingCycle { get; private set; }
    public int? MinAge { get; private set; }
    public int? MaxAge { get; private set; }
    public int? Capacity { get; private set; }
    public decimal? JoiningFee { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool AllowAutoRenewal { get; private set; } = true;
    public decimal? FreezeFee { get; private set; }

    public static MembershipType Create(Guid clubId, string name, decimal price, BillingCycle cycle)
    {
        return new MembershipType
        {
            ClubId = clubId,
            Name = name,
            Price = price,
            BillingCycle = cycle
        };
    }

    public void Update(
        string name,
        string? description,
        decimal price,
        BillingCycle billingCycle,
        int? minAge,
        int? maxAge,
        int? capacity,
        decimal? joiningFee,
        bool isActive,
        bool allowAutoRenewal,
        decimal? freezeFee)
    {
        Name = name;
        Description = description;
        Price = price;
        BillingCycle = billingCycle;
        MinAge = minAge;
        MaxAge = maxAge;
        Capacity = capacity;
        JoiningFee = joiningFee;
        IsActive = isActive;
        AllowAutoRenewal = allowAutoRenewal;
        FreezeFee = freezeFee;
        UpdatedAt = DateTime.UtcNow;
    }
}
