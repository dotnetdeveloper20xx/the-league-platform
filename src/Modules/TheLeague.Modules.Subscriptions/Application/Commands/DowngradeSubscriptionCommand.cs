using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Subscriptions.Application.Dtos;
using TheLeague.Modules.Subscriptions.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Enums;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Subscriptions.Application.Commands;

public record DowngradeSubscriptionCommand(Guid ClubId, SubscriptionTier NewTier) : IRequest<Result<SubscriptionDto>>;

public class DowngradeSubscriptionCommandHandler : IRequestHandler<DowngradeSubscriptionCommand, Result<SubscriptionDto>>
{
    private readonly SubscriptionsDbContext _db;

    public DowngradeSubscriptionCommandHandler(SubscriptionsDbContext db)
    {
        _db = db;
    }

    public async Task<Result<SubscriptionDto>> Handle(DowngradeSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var subscription = await _db.ClubSubscriptions
            .FirstOrDefaultAsync(s => s.ClubId == request.ClubId, cancellationToken);

        if (subscription is null)
            return Result.Failure<SubscriptionDto>("Subscription not found.");

        if (request.NewTier >= subscription.CurrentTier)
            return Result.Failure<SubscriptionDto>("New tier must be lower than current tier. Use upgrade for higher tiers.");

        subscription.ScheduleDowngrade(request.NewTier);
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

        return Result.Success(dto, $"Downgrade scheduled for {subscription.ScheduledDowngradeDate:yyyy-MM-dd}.");
    }
}
