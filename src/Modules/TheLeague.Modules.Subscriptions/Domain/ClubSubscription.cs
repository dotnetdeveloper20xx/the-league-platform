using TheLeague.Shared.Domain.Entities;
using TheLeague.Shared.Domain.Enums;

namespace TheLeague.Modules.Subscriptions.Domain;

public class ClubSubscription : BaseEntity
{
    public Guid ClubId { get; private set; }
    public SubscriptionTier CurrentTier { get; private set; } = SubscriptionTier.Free;
    public SubscriptionTier? ScheduledDowngradeTier { get; private set; }
    public DateTime? ScheduledDowngradeDate { get; private set; }
    public DateTime BillingPeriodStart { get; private set; }
    public DateTime BillingPeriodEnd { get; private set; }
    public bool IsTrialActive { get; private set; }
    public DateTime? TrialStartDate { get; private set; }
    public DateTime? TrialEndDate { get; private set; }
    public string? StripeSubscriptionId { get; private set; }
    public string? StripeCustomerId { get; private set; }
    public int FailedPaymentAttempts { get; private set; }
    public DateTime? LastPaymentFailureDate { get; private set; }

    public static ClubSubscription CreateWithTrial(Guid clubId, SubscriptionTier selectedTier)
    {
        var now = DateTime.UtcNow;
        return new ClubSubscription
        {
            ClubId = clubId,
            CurrentTier = SubscriptionTier.Pro, // Trial gets Pro access
            IsTrialActive = true,
            TrialStartDate = now,
            TrialEndDate = now.AddDays(14),
            BillingPeriodStart = now,
            BillingPeriodEnd = now.AddDays(14),
            ScheduledDowngradeTier = selectedTier // Will activate this after trial
        };
    }

    public static ClubSubscription CreateFree(Guid clubId)
    {
        var now = DateTime.UtcNow;
        return new ClubSubscription
        {
            ClubId = clubId,
            CurrentTier = SubscriptionTier.Free,
            BillingPeriodStart = now,
            BillingPeriodEnd = now.AddMonths(1)
        };
    }

    public void Upgrade(SubscriptionTier newTier)
    {
        CurrentTier = newTier;
        ScheduledDowngradeTier = null;
        ScheduledDowngradeDate = null;
        FailedPaymentAttempts = 0;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ScheduleDowngrade(SubscriptionTier newTier)
    {
        ScheduledDowngradeTier = newTier;
        ScheduledDowngradeDate = BillingPeriodEnd;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ApplyDowngrade()
    {
        if (ScheduledDowngradeTier.HasValue)
        {
            CurrentTier = ScheduledDowngradeTier.Value;
            ScheduledDowngradeTier = null;
            ScheduledDowngradeDate = null;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void RecordPaymentFailure()
    {
        FailedPaymentAttempts++;
        LastPaymentFailureDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void DowngradeToFree()
    {
        CurrentTier = SubscriptionTier.Free;
        FailedPaymentAttempts = 0;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ActivateAfterTrial(SubscriptionTier tier)
    {
        IsTrialActive = false;
        CurrentTier = tier;
        BillingPeriodStart = DateTime.UtcNow;
        BillingPeriodEnd = DateTime.UtcNow.AddMonths(1);
        ScheduledDowngradeTier = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ExpireTrial()
    {
        IsTrialActive = false;
        CurrentTier = SubscriptionTier.Free;
        ScheduledDowngradeTier = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public decimal CalculateProration(decimal newPrice, decimal oldPrice)
    {
        var totalDays = (BillingPeriodEnd - BillingPeriodStart).TotalDays;
        var remainingDays = (BillingPeriodEnd - DateTime.UtcNow).TotalDays;
        if (totalDays <= 0) return 0;
        return Math.Round((newPrice - oldPrice) * (decimal)(remainingDays / totalDays), 2);
    }
}
