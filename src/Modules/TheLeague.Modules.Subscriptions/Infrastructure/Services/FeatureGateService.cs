using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Subscriptions.Infrastructure.Persistence;

namespace TheLeague.Modules.Subscriptions.Infrastructure.Services;

public class FeatureGateService : IFeatureGateService
{
    private readonly SubscriptionsDbContext _db;

    public FeatureGateService(SubscriptionsDbContext db)
    {
        _db = db;
    }

    public async Task<bool> IsFeatureEnabledAsync(Guid clubId, string featureKey, CancellationToken ct = default)
    {
        var subscription = await _db.ClubSubscriptions
            .FirstOrDefaultAsync(s => s.ClubId == clubId, ct);

        if (subscription is null) return false;

        var tierConfig = await _db.TierConfigs
            .FirstOrDefaultAsync(t => t.Tier == subscription.CurrentTier, ct);

        if (tierConfig?.Features is null) return false;

        return tierConfig.Features.Contains(featureKey);
    }

    public async Task<bool> CanAddMemberAsync(Guid clubId, CancellationToken ct = default)
    {
        var subscription = await _db.ClubSubscriptions
            .FirstOrDefaultAsync(s => s.ClubId == clubId, ct);

        if (subscription is null) return false;

        var tierConfig = await _db.TierConfigs
            .FirstOrDefaultAsync(t => t.Tier == subscription.CurrentTier, ct);

        if (tierConfig is null) return false;

        var usage = await _db.UsageRecords
            .Where(u => u.ClubId == clubId && u.PeriodEnd > DateTime.UtcNow)
            .OrderByDescending(u => u.PeriodStart)
            .FirstOrDefaultAsync(ct);

        if (usage is null) return true; // No usage record yet, allow

        return usage.CurrentMemberCount < tierConfig.MaxMembers;
    }

    public async Task<bool> CanUploadAsync(Guid clubId, long fileSizeBytes, CancellationToken ct = default)
    {
        var subscription = await _db.ClubSubscriptions
            .FirstOrDefaultAsync(s => s.ClubId == clubId, ct);

        if (subscription is null) return false;

        var tierConfig = await _db.TierConfigs
            .FirstOrDefaultAsync(t => t.Tier == subscription.CurrentTier, ct);

        if (tierConfig is null) return false;

        var usage = await _db.UsageRecords
            .Where(u => u.ClubId == clubId && u.PeriodEnd > DateTime.UtcNow)
            .OrderByDescending(u => u.PeriodStart)
            .FirstOrDefaultAsync(ct);

        if (usage is null) return fileSizeBytes <= tierConfig.MaxStorageBytes;

        return (usage.CurrentStorageBytes + fileSizeBytes) <= tierConfig.MaxStorageBytes;
    }

    public async Task<bool> CanSendSmsAsync(Guid clubId, CancellationToken ct = default)
    {
        var subscription = await _db.ClubSubscriptions
            .FirstOrDefaultAsync(s => s.ClubId == clubId, ct);

        if (subscription is null) return false;

        var tierConfig = await _db.TierConfigs
            .FirstOrDefaultAsync(t => t.Tier == subscription.CurrentTier, ct);

        if (tierConfig is null) return false;

        var usage = await _db.UsageRecords
            .Where(u => u.ClubId == clubId && u.PeriodEnd > DateTime.UtcNow)
            .OrderByDescending(u => u.PeriodStart)
            .FirstOrDefaultAsync(ct);

        if (usage is null) return tierConfig.MonthlySmsCredits > 0;

        return usage.CurrentMonthlySmsUsed < tierConfig.MonthlySmsCredits;
    }
}
