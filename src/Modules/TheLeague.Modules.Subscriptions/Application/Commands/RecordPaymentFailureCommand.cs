using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Subscriptions.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Subscriptions.Application.Commands;

public record RecordPaymentFailureCommand(Guid ClubId) : IRequest<Result>;

public class RecordPaymentFailureCommandHandler : IRequestHandler<RecordPaymentFailureCommand, Result>
{
    private readonly SubscriptionsDbContext _db;
    private const int MaxRetryAttempts = 3; // Retry at days 1, 3, 7 — 3 total attempts

    public RecordPaymentFailureCommandHandler(SubscriptionsDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(RecordPaymentFailureCommand request, CancellationToken cancellationToken)
    {
        var subscription = await _db.ClubSubscriptions
            .FirstOrDefaultAsync(s => s.ClubId == request.ClubId, cancellationToken);

        if (subscription is null)
            return Result.Failure("Subscription not found.");

        subscription.RecordPaymentFailure();

        // Dunning logic: after 3 failed attempts (days 1, 3, 7), downgrade to Free
        if (subscription.FailedPaymentAttempts >= MaxRetryAttempts)
        {
            subscription.DowngradeToFree();
            await _db.SaveChangesAsync(cancellationToken);
            return Result.Success("All retry attempts exhausted. Club downgraded to Free tier.");
        }

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success($"Payment failure recorded. Attempt {subscription.FailedPaymentAttempts} of {MaxRetryAttempts}.");
    }
}
