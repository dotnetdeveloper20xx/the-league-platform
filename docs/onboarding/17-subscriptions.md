# 17 — Subscription Tiers & Feature Gating

## 📖 Feature Overview

The Subscriptions module manages the platform's SaaS business model — clubs subscribe to tiers (Free, Starter, Pro, Enterprise) that gate features and enforce usage limits. It handles trials, upgrades with proration, downgrades at period end, dunning for failed payments, and usage tracking.

### Key Capabilities
- 4 subscription tiers with distinct pricing and limits
- 14-day Pro trial for new clubs
- Upgrade with proration calculation (credit for unused days)
- Downgrade scheduling (takes effect at period end)
- Dunning process (retry at days 1, 3, 7; downgrade on final failure)
- Usage tracking (members, storage, SMS messages)
- Feature gate service for checking tier limits
- Usage limit enforcement with graceful degradation

---

## 🏗️ Architecture & Design Decisions

| Decision | Rationale |
|----------|-----------|
| 4 fixed tiers (not custom) | Simple pricing page; predictable revenue; easy to explain |
| Pro trial (14 days) | Lets clubs experience full features before committing |
| Proration on upgrade | Fair billing; member only pays for remaining days at new rate |
| Downgrade at period end | Prevents mid-period feature loss; honours paid period |
| 3-attempt dunning | Industry standard; gives time to fix payment issues |
| Usage records per billing period | Clear tracking; resets each period; historical data retained |
| Feature gate as service | Centralised check; used by all modules before feature access |

---

## 📊 Subscription Tiers

### Tier Configuration
```
┌──────────────┬──────────┬──────────┬──────────┬─────────────┐
│ Feature      │  Free    │ Starter  │   Pro    │ Enterprise  │
├──────────────┼──────────┼──────────┼──────────┼─────────────┤
│ Price/month  │  £0      │  £29     │  £79     │  £199       │
│ Price/year   │  £0      │  £290    │  £790    │  £1,990     │
│ Members      │  25      │  100     │  500     │  Unlimited  │
│ Storage (GB) │  1       │  5       │  25      │  100        │
│ SMS/month    │  0       │  100     │  500     │  2,000      │
│ Admins       │  1       │  3       │  10      │  Unlimited  │
│ Competitions │  1       │  5       │  Unlim.  │  Unlimited  │
│ Analytics    │  Basic   │  Standard│  Advanced│  Advanced   │
│ API Access   │  ✗       │  ✗       │  ✓       │  ✓          │
│ Custom Brand │  ✗       │  ✗       │  ✓       │  ✓          │
│ Priority Sup │  ✗       │  ✗       │  ✗       │  ✓          │
│ Webhooks     │  ✗       │  ✗       │  ✓       │  ✓          │
└──────────────┴──────────┴──────────┴──────────┴─────────────┘
```

### SubscriptionTierConfig Entity
```csharp
public class SubscriptionTierConfig : BaseEntity
{
    public SubscriptionTier Tier { get; private set; }
    public string Name { get; private set; }              // "Free", "Starter", "Pro", "Enterprise"
    public decimal MonthlyPrice { get; private set; }
    public decimal AnnualPrice { get; private set; }      // ~2 months free on annual
    public int MaxMembers { get; private set; }           // 0 = unlimited
    public int MaxStorageGb { get; private set; }
    public int MaxSmsPerMonth { get; private set; }
    public int MaxAdmins { get; private set; }            // 0 = unlimited
    public int MaxCompetitions { get; private set; }      // 0 = unlimited
    public bool HasAdvancedAnalytics { get; private set; }
    public bool HasApiAccess { get; private set; }
    public bool HasCustomBranding { get; private set; }
    public bool HasPrioritySupport { get; private set; }
    public bool HasWebhooks { get; private set; }
}

public enum SubscriptionTier
{
    Free,
    Starter,
    Pro,
    Enterprise
}
```

### ClubSubscription Entity
```csharp
public class ClubSubscription : TenantEntity
{
    public SubscriptionTier Tier { get; private set; }
    public SubscriptionStatus Status { get; private set; }
    public BillingInterval BillingInterval { get; private set; } // Monthly or Annual
    public DateTime StartDate { get; private set; }
    public DateTime CurrentPeriodStart { get; private set; }
    public DateTime CurrentPeriodEnd { get; private set; }
    public decimal CurrentPrice { get; private set; }
    public bool IsTrialing { get; private set; }
    public DateTime? TrialEndDate { get; private set; }
    public SubscriptionTier? ScheduledDowngradeTo { get; private set; } // Pending downgrade
    public int FailedPaymentAttempts { get; private set; }
    public string? ExternalSubscriptionId { get; private set; } // Stripe subscription ID
}

public enum SubscriptionStatus
{
    Active,         // Paid and current
    Trialing,       // In trial period
    PastDue,        // Payment failed, in dunning
    Cancelled,      // Subscription ended
    Paused          // Temporarily paused (admin action)
}

public enum BillingInterval { Monthly, Annual }
```

---

## 🆓 Trial Support

### 14-Day Pro Trial
```csharp
public static ClubSubscription CreateTrial(Guid clubId)
{
    return new ClubSubscription
    {
        ClubId = clubId,
        Tier = SubscriptionTier.Pro,          // Trial gives Pro features
        Status = SubscriptionStatus.Trialing,
        BillingInterval = BillingInterval.Monthly,
        StartDate = DateTime.UtcNow,
        CurrentPeriodStart = DateTime.UtcNow,
        CurrentPeriodEnd = DateTime.UtcNow.AddDays(14),
        CurrentPrice = 0m,                     // Free during trial
        IsTrialing = true,
        TrialEndDate = DateTime.UtcNow.AddDays(14)
    };
}
```

**Trial rules:**
- Every new club gets a 14-day Pro trial automatically
- No payment method required during trial
- Full Pro features available during trial
- At trial end: club must choose a paid tier or drops to Free
- Trial can only be used once per club

---

## ⬆️ Upgrade with Proration

### Proration Calculation
```csharp
public class ProrationCalculator
{
    public ProrationResult Calculate(ClubSubscription current,
        SubscriptionTierConfig newTier, DateTime upgradeDate)
    {
        // Days remaining in current period
        var totalDaysInPeriod = (current.CurrentPeriodEnd - current.CurrentPeriodStart).TotalDays;
        var daysRemaining = (current.CurrentPeriodEnd - upgradeDate).TotalDays;

        // Credit for unused days at current rate
        var dailyRateCurrent = current.CurrentPrice / (decimal)totalDaysInPeriod;
        var credit = Math.Round(dailyRateCurrent * (decimal)daysRemaining, 2);

        // Charge for remaining days at new rate
        var newPrice = current.BillingInterval == BillingInterval.Monthly
            ? newTier.MonthlyPrice
            : newTier.AnnualPrice / 12; // Monthly equivalent for annual

        var dailyRateNew = newPrice / (decimal)totalDaysInPeriod;
        var charge = Math.Round(dailyRateNew * (decimal)daysRemaining, 2);

        // Net amount to charge (charge - credit)
        var netCharge = charge - credit;

        return new ProrationResult(credit, charge, netCharge);
    }
}
```

**Example:**
- Club on Starter (£29/month), upgrades to Pro (£79/month) on day 15 of 30
- Credit for unused Starter: £29 × (15/30) = £14.50
- Charge for Pro remainder: £79 × (15/30) = £39.50
- Net charge: £39.50 - £14.50 = £25.00

---

## ⬇️ Downgrade Scheduling

```csharp
public void ScheduleDowngrade(SubscriptionTier newTier)
{
    if (newTier >= Tier)
        throw new InvalidOperationException("Cannot schedule downgrade to same or higher tier.");

    // Downgrade takes effect at end of current billing period
    ScheduledDowngradeTo = newTier;

    // Note: Features remain available until CurrentPeriodEnd
}

// Background job runs daily
public async Task ProcessScheduledDowngrades()
{
    var due = await _db.ClubSubscriptions
        .Where(s => s.ScheduledDowngradeTo != null
            && s.CurrentPeriodEnd <= DateTime.UtcNow)
        .ToListAsync();

    foreach (var sub in due)
    {
        sub.ApplyDowngrade();
        await _mediator.Publish(new SubscriptionDowngradedEvent(
            sub.ClubId, sub.Tier, sub.ScheduledDowngradeTo.Value));
    }
}
```

---

## 🔔 Dunning (Failed Payment Recovery)

### Dunning Process
```
Payment fails
     │
     ▼
┌─────────────────────────────────────────────────────────┐
│ Day 1: First retry                                       │
│   → Retry payment                                        │
│   → Send "Payment failed" email to club admin            │
│   → Status: PastDue                                      │
├─────────────────────────────────────────────────────────┤
│ Day 3: Second retry                                      │
│   → Retry payment                                        │
│   → Send "Action required" email with update card link   │
├─────────────────────────────────────────────────────────┤
│ Day 7: Final retry                                       │
│   → Retry payment                                        │
│   → If fails: downgrade to Free tier                     │
│   → Send "Subscription downgraded" email                 │
│   → Status: Active (at Free tier)                        │
└─────────────────────────────────────────────────────────┘
```

### Dunning Implementation
```csharp
public class DunningService
{
    private static readonly int[] RetryDays = { 1, 3, 7 };

    public async Task ProcessFailedPayment(ClubSubscription subscription)
    {
        subscription.FailedPaymentAttempts++;
        subscription.Status = SubscriptionStatus.PastDue;

        if (subscription.FailedPaymentAttempts > RetryDays.Length)
        {
            // Final failure: downgrade to Free
            subscription.Tier = SubscriptionTier.Free;
            subscription.Status = SubscriptionStatus.Active;
            subscription.FailedPaymentAttempts = 0;

            await _mediator.Publish(new SubscriptionDowngradedEvent(
                subscription.ClubId, subscription.Tier, SubscriptionTier.Free));
        }
        else
        {
            // Schedule next retry
            var nextRetryDay = RetryDays[subscription.FailedPaymentAttempts - 1];
            await _scheduler.ScheduleRetry(subscription.Id,
                DateTime.UtcNow.AddDays(nextRetryDay));
        }
    }
}
```

---

## 📊 Usage Tracking

### UsageRecord Entity
```csharp
public class UsageRecord : TenantEntity
{
    public UsageMetric Metric { get; private set; }
    public int CurrentValue { get; private set; }
    public int Limit { get; private set; }                // From tier config (0 = unlimited)
    public DateTime PeriodStart { get; private set; }
    public DateTime PeriodEnd { get; private set; }
    public DateTime LastUpdated { get; private set; }
}

public enum UsageMetric
{
    Members,        // Active member count
    StorageGb,      // Storage used in GB
    SmsMessages,    // SMS sent this period
    Admins,         // Admin user count
    Competitions    // Active competitions
}
```

### IFeatureGateService
```csharp
public interface IFeatureGateService
{
    Task<bool> CanAccessFeature(Guid clubId, string featureName);
    Task<UsageLimitResult> CheckUsageLimit(Guid clubId, UsageMetric metric);
    Task<SubscriptionTier> GetCurrentTier(Guid clubId);
}

public class FeatureGateService : IFeatureGateService
{
    public async Task<UsageLimitResult> CheckUsageLimit(Guid clubId, UsageMetric metric)
    {
        var subscription = await GetSubscription(clubId);
        var tierConfig = await GetTierConfig(subscription.Tier);
        var usage = await GetCurrentUsage(clubId, metric);

        var limit = metric switch
        {
            UsageMetric.Members => tierConfig.MaxMembers,
            UsageMetric.StorageGb => tierConfig.MaxStorageGb,
            UsageMetric.SmsMessages => tierConfig.MaxSmsPerMonth,
            UsageMetric.Admins => tierConfig.MaxAdmins,
            UsageMetric.Competitions => tierConfig.MaxCompetitions,
            _ => 0
        };

        if (limit == 0) // Unlimited
            return UsageLimitResult.Allowed(usage.CurrentValue, limit);

        if (usage.CurrentValue >= limit)
            return UsageLimitResult.LimitReached(usage.CurrentValue, limit);

        // Warn at 80% usage
        if (usage.CurrentValue >= limit * 0.8m)
            return UsageLimitResult.NearingLimit(usage.CurrentValue, limit);

        return UsageLimitResult.Allowed(usage.CurrentValue, limit);
    }

    public async Task<bool> CanAccessFeature(Guid clubId, string featureName)
    {
        var subscription = await GetSubscription(clubId);
        var tierConfig = await GetTierConfig(subscription.Tier);

        return featureName switch
        {
            "AdvancedAnalytics" => tierConfig.HasAdvancedAnalytics,
            "ApiAccess" => tierConfig.HasApiAccess,
            "CustomBranding" => tierConfig.HasCustomBranding,
            "PrioritySupport" => tierConfig.HasPrioritySupport,
            "Webhooks" => tierConfig.HasWebhooks,
            _ => true // Unknown features default to allowed
        };
    }
}
```

---

## 🚧 Usage Limit Enforcement

```csharp
// Example: Enforcing member limit when adding a new member
public class CreateMemberCommandHandler
{
    public async Task<Result> Handle(CreateMemberCommand command)
    {
        var limitCheck = await _featureGate.CheckUsageLimit(
            command.ClubId, UsageMetric.Members);

        if (limitCheck.Status == UsageLimitStatus.LimitReached)
            return Result.Failure(
                $"Member limit reached ({limitCheck.Current}/{limitCheck.Limit}). " +
                "Upgrade your subscription to add more members.");

        // Proceed with member creation...
    }
}
```

---

## 🌐 API Endpoints

| Method | Route | Permission | Purpose |
|--------|-------|-----------|---------|
| GET | /api/v1/subscriptions/current | ViewMembers | Get current subscription |
| GET | /api/v1/subscriptions/tiers | Public | List available tiers |
| POST | /api/v1/subscriptions/upgrade | ManageMembers | Upgrade tier |
| POST | /api/v1/subscriptions/downgrade | ManageMembers | Schedule downgrade |
| GET | /api/v1/subscriptions/usage | ViewMembers | Get usage summary |
| POST | /api/v1/subscriptions/cancel | ManageMembers | Cancel subscription |
| GET | /api/v1/subscriptions/proration | ViewMembers | Preview upgrade cost |
| PUT | /api/v1/subscriptions/billing-interval | ManageMembers | Switch monthly/annual |

---

## 🧪 Testing Approach

### Property Tests
```
Property 30: Usage Never Exceeds Limit
  For ANY club on ANY tier, the usage for each metric
  SHALL never exceed the tier's configured limit (when limit > 0).

Property 31: Proration Fairness
  For ANY upgrade, the net proration charge SHALL be >= 0
  AND <= the full price of the new tier for the remaining period.

Property 32: Dunning Terminates
  For ANY subscription in dunning, the process SHALL terminate
  after exactly 3 retry attempts (downgrade to Free on final failure).
```

### Unit Tests
- New club → gets 14-day Pro trial
- Upgrade Starter→Pro mid-period → correct proration calculated
- Downgrade Pro→Starter → scheduled for period end, not immediate
- 3 failed payments → downgraded to Free
- Check member limit on Free (25 members, 25 exist) → LimitReached
- Check member limit on Enterprise → always Allowed (unlimited)
- Feature gate: API access on Free → false
- Feature gate: API access on Pro → true
- Trial expires without payment → drops to Free

---

## 🚀 How to Extend

### Adding custom enterprise pricing:
1. Add `CustomPricing` entity linked to club
2. Override tier config values for specific clubs
3. Support custom limits negotiated with sales team

### Adding usage alerts:
1. Check usage at 80% and 90% thresholds
2. Send email notification to club admins
3. Show in-app banner when nearing limits
