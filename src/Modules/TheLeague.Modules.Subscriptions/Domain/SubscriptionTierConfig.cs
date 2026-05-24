using TheLeague.Shared.Domain.Entities;
using TheLeague.Shared.Domain.Enums;

namespace TheLeague.Modules.Subscriptions.Domain;

public class SubscriptionTierConfig : BaseEntity
{
    public SubscriptionTier Tier { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public decimal MonthlyPrice { get; private set; }
    public int MaxMembers { get; private set; }
    public long MaxStorageBytes { get; private set; }
    public int MonthlySmsCredits { get; private set; }
    public string? Features { get; private set; } // JSON array of enabled feature keys

    public static List<SubscriptionTierConfig> GetDefaults() => new()
    {
        new() { Tier = SubscriptionTier.Free, Name = "Free", MonthlyPrice = 0, MaxMembers = 50, MaxStorageBytes = 1L * 1024 * 1024 * 1024, MonthlySmsCredits = 0 },
        new() { Tier = SubscriptionTier.Starter, Name = "Starter", MonthlyPrice = 29, MaxMembers = 200, MaxStorageBytes = 5L * 1024 * 1024 * 1024, MonthlySmsCredits = 500 },
        new() { Tier = SubscriptionTier.Pro, Name = "Pro", MonthlyPrice = 79, MaxMembers = 1000, MaxStorageBytes = 25L * 1024 * 1024 * 1024, MonthlySmsCredits = 2000 },
        new() { Tier = SubscriptionTier.Enterprise, Name = "Enterprise", MonthlyPrice = 199, MaxMembers = int.MaxValue, MaxStorageBytes = 100L * 1024 * 1024 * 1024, MonthlySmsCredits = 10000 }
    };
}
