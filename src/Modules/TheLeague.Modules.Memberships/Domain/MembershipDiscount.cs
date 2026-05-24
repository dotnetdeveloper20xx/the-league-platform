using TheLeague.Shared.Domain.Entities;
using TheLeague.Shared.Domain.Enums;

namespace TheLeague.Modules.Memberships.Domain;

public class MembershipDiscount : TenantEntity
{
    public Guid? MembershipTypeId { get; private set; }
    public DiscountType DiscountType { get; private set; }
    public bool IsPercentage { get; private set; }
    public decimal Value { get; private set; }
    public DateTime ValidFrom { get; private set; }
    public DateTime ValidTo { get; private set; }
    public string? PromoCode { get; private set; }
    public int? MaxUses { get; private set; }
    public int CurrentUses { get; private set; }

    public static MembershipDiscount Create(
        Guid clubId,
        Guid? membershipTypeId,
        DiscountType discountType,
        bool isPercentage,
        decimal value,
        DateTime validFrom,
        DateTime validTo,
        string? promoCode = null,
        int? maxUses = null)
    {
        return new MembershipDiscount
        {
            ClubId = clubId,
            MembershipTypeId = membershipTypeId,
            DiscountType = discountType,
            IsPercentage = isPercentage,
            Value = value,
            ValidFrom = validFrom,
            ValidTo = validTo,
            PromoCode = promoCode,
            MaxUses = maxUses,
            CurrentUses = 0
        };
    }

    public bool IsValid(DateTime asOf)
    {
        if (asOf < ValidFrom || asOf > ValidTo) return false;
        if (MaxUses.HasValue && CurrentUses >= MaxUses.Value) return false;
        return true;
    }

    public void IncrementUses()
    {
        CurrentUses++;
        UpdatedAt = DateTime.UtcNow;
    }

    public decimal CalculateDiscount(decimal basePrice)
    {
        if (IsPercentage)
        {
            var discount = Math.Round(basePrice * Value / 100m, 2);
            return Math.Min(discount, basePrice);
        }

        return Math.Min(Value, basePrice);
    }
}
