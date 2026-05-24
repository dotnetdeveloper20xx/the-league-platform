using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Subscriptions.Application.Dtos;
using TheLeague.Modules.Subscriptions.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Subscriptions.Application.Queries;

public record GetSubscriptionQuery(Guid ClubId) : IRequest<Result<SubscriptionDto>>;

public class GetSubscriptionQueryHandler : IRequestHandler<GetSubscriptionQuery, Result<SubscriptionDto>>
{
    private readonly SubscriptionsDbContext _db;

    public GetSubscriptionQueryHandler(SubscriptionsDbContext db)
    {
        _db = db;
    }

    public async Task<Result<SubscriptionDto>> Handle(GetSubscriptionQuery request, CancellationToken cancellationToken)
    {
        var subscription = await _db.ClubSubscriptions
            .FirstOrDefaultAsync(s => s.ClubId == request.ClubId, cancellationToken);

        if (subscription is null)
            return Result.Failure<SubscriptionDto>("Subscription not found.");

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

        return Result.Success(dto);
    }
}
