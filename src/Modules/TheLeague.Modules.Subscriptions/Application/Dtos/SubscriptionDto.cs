using TheLeague.Shared.Domain.Enums;

namespace TheLeague.Modules.Subscriptions.Application.Dtos;

public record SubscriptionDto(
    Guid Id,
    Guid ClubId,
    SubscriptionTier CurrentTier,
    SubscriptionTier? ScheduledDowngradeTier,
    DateTime? ScheduledDowngradeDate,
    DateTime BillingPeriodStart,
    DateTime BillingPeriodEnd,
    bool IsTrialActive,
    DateTime? TrialEndDate,
    int FailedPaymentAttempts
);

public record UsageDto(
    int CurrentMemberCount,
    int MaxMembers,
    long CurrentStorageBytes,
    long MaxStorageBytes,
    int CurrentMonthlySmsUsed,
    int MonthlySmsCredits,
    DateTime PeriodStart,
    DateTime PeriodEnd
);

public record TierDto(
    SubscriptionTier Tier,
    string Name,
    decimal MonthlyPrice,
    int MaxMembers,
    long MaxStorageBytes,
    int MonthlySmsCredits,
    string? Features
);

public record UsageLimitCheckDto(
    string LimitType,
    bool IsExceeded,
    string? Message
);
