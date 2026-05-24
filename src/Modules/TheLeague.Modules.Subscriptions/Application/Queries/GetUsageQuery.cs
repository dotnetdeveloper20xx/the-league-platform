using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Subscriptions.Application.Dtos;
using TheLeague.Modules.Subscriptions.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Subscriptions.Application.Queries;

public record GetUsageQuery(Guid ClubId) : IRequest<Result<UsageDto>>;

public class GetUsageQueryHandler : IRequestHandler<GetUsageQuery, Result<UsageDto>>
{
    private readonly SubscriptionsDbContext _db;

    public GetUsageQueryHandler(SubscriptionsDbContext db)
    {
        _db = db;
    }

    public async Task<Result<UsageDto>> Handle(GetUsageQuery request, CancellationToken cancellationToken)
    {
        var subscription = await _db.ClubSubscriptions
            .FirstOrDefaultAsync(s => s.ClubId == request.ClubId, cancellationToken);

        if (subscription is null)
            return Result.Failure<UsageDto>("Subscription not found.");

        var tierConfig = await _db.TierConfigs
            .FirstOrDefaultAsync(t => t.Tier == subscription.CurrentTier, cancellationToken);

        if (tierConfig is null)
            return Result.Failure<UsageDto>("Tier configuration not found.");

        var usage = await _db.UsageRecords
            .Where(u => u.ClubId == request.ClubId && u.PeriodEnd > DateTime.UtcNow)
            .OrderByDescending(u => u.PeriodStart)
            .FirstOrDefaultAsync(cancellationToken);

        var dto = new UsageDto(
            usage?.CurrentMemberCount ?? 0,
            tierConfig.MaxMembers,
            usage?.CurrentStorageBytes ?? 0,
            tierConfig.MaxStorageBytes,
            usage?.CurrentMonthlySmsUsed ?? 0,
            tierConfig.MonthlySmsCredits,
            usage?.PeriodStart ?? subscription.BillingPeriodStart,
            usage?.PeriodEnd ?? subscription.BillingPeriodEnd
        );

        return Result.Success(dto);
    }
}
