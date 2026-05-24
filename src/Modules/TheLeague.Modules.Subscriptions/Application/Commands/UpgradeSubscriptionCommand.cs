using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Subscriptions.Application.Dtos;
using TheLeague.Modules.Subscriptions.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Enums;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Subscriptions.Application.Commands;

public record UpgradeSubscriptionCommand(Guid ClubId, SubscriptionTier NewTier) : IRequest<Result<SubscriptionDto>>;

public class UpgradeSubscriptionCommandHandler : IRequestHandler<UpgradeSubscriptionCommand, Result<SubscriptionDto>>
{
    private readonly SubscriptionsDbContext _db;

    public UpgradeSubscriptionCommandHandler(SubscriptionsDbContext db)
    {
        _db = db;
    }

    public async Task<Result<SubscriptionDto>> Handle(UpgradeSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var subscription = await _db.ClubSubscriptions
            .FirstOrDefaultAsync(s => s.ClubId == request.ClubId, cancellationToken);

        if (subscription is null)
            return Result.Failure<SubscriptionDto>("Subscription not found.");

        if (request.NewTier <= subscription.CurrentTier)
            return Result.Failure<SubscriptionDto>("New tier must be higher than current tier. Use downgrade for lower tiers.");

        var oldTierConfig = await _db.TierConfigs
            .FirstOrDefaultAsync(t => t.Tier == subscription.CurrentTier, cancellationToken);

        var newTierConfig = await _db.TierConfigs
            .FirstOrDefaultAsync(t => t.Tier == request.NewTier, cancellationToken);

        if (newTierConfig is null)
            return Result.Failure<SubscriptionDto>("Invalid tier configuration.");

        // Calculate proration: (PB - PA) × remaining days / total days
        decimal prorationAmount = 0;
        if (oldTierConfig is not null)
        {
            prorationAmount = subscription.CalculateProration(newTierConfig.MonthlyPrice, oldTierConfig.MonthlyPrice);
        }

        subscription.Upgrade(request.NewTier);
        await _db.SaveChangesAsync(cancellationToken);

        var dto = new SubscriptionDto(
            subscription.Id,
            subscription.ClubId,
            subscription.CurrentTier,
            subscription.ScheduledDowngradeTier,
            subscription.ScheduledDowngradeDate,
            subscription.BillingPeriodStart,
            subscription.BillingPeriodEnd,
            subscription.IsTrialActive,
            subscription.TrialEndDate,
            subscription.FailedPaymentAttempts
        );

        return Result.Success(dto, $"Upgrade successful. Prorated charge: £{prorationAmount}");
    }
}
