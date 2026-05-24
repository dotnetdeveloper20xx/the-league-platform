using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Subscriptions.Application.Dtos;
using TheLeague.Modules.Subscriptions.Domain;
using TheLeague.Modules.Subscriptions.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Enums;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Subscriptions.Application.Commands;

public record CreateSubscriptionCommand(Guid ClubId, SubscriptionTier SelectedTier, bool WithTrial = true) : IRequest<Result<SubscriptionDto>>;

public class CreateSubscriptionCommandHandler : IRequestHandler<CreateSubscriptionCommand, Result<SubscriptionDto>>
{
    private readonly SubscriptionsDbContext _db;

    public CreateSubscriptionCommandHandler(SubscriptionsDbContext db)
    {
        _db = db;
    }

    public async Task<Result<SubscriptionDto>> Handle(CreateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var existing = await _db.ClubSubscriptions
            .FirstOrDefaultAsync(s => s.ClubId == request.ClubId, cancellationToken);

        if (existing is not null)
            return Result.Failure<SubscriptionDto>("Club already has a subscription.");

        ClubSubscription subscription;

        if (request.SelectedTier == SubscriptionTier.Free)
        {
            subscription = ClubSubscription.CreateFree(request.ClubId);
        }
        else if (request.WithTrial)
        {
            subscription = ClubSubscription.CreateWithTrial(request.ClubId, request.SelectedTier);
        }
        else
        {
            subscription = ClubSubscription.CreateFree(request.ClubId);
            subscription.Upgrade(request.SelectedTier);
        }

        _db.ClubSubscriptions.Add(subscription);

        // Create initial usage record
        var usage = UsageRecord.Create(request.ClubId);
        _db.UsageRecords.Add(usage);

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

        return Result.Success(dto);
    }
}
