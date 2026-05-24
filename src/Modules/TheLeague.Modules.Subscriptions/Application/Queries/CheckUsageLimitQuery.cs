using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Subscriptions.Application.Dtos;
using TheLeague.Modules.Subscriptions.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Subscriptions.Application.Queries;

public record CheckUsageLimitQuery(Guid ClubId, string LimitType) : IRequest<Result<UsageLimitCheckDto>>;

public class CheckUsageLimitQueryHandler : IRequestHandler<CheckUsageLimitQuery, Result<UsageLimitCheckDto>>
{
    private readonly SubscriptionsDbContext _db;

    public CheckUsageLimitQueryHandler(SubscriptionsDbContext db)
    {
        _db = db;
    }

    public async Task<Result<UsageLimitCheckDto>> Handle(CheckUsageLimitQuery request, CancellationToken cancellationToken)
    {
        var subscription = await _db.ClubSubscriptions
            .FirstOrDefaultAsync(s => s.ClubId == request.ClubId, cancellationToken);

        if (subscription is null)
            return Result.Failure<UsageLimitCheckDto>("Subscription not found.");

        var tierConfig = await _db.TierConfigs
            .FirstOrDefaultAsync(t => t.Tier == subscription.CurrentTier, cancellationToken);

        if (tierConfig is null)
            return Result.Failure<UsageLimitCheckDto>("Tier configuration not found.");

        var usage = await _db.UsageRecords
            .Where(u => u.ClubId == request.ClubId && u.PeriodEnd > DateTime.UtcNow)
            .OrderByDescending(u => u.PeriodStart)
            .FirstOrDefaultAsync(cancellationToken);

        bool isExceeded;
        string? message;

        switch (request.LimitType.ToLowerInvariant())
        {
            case "members":
                isExceeded = usage is not null && usage.CurrentMemberCount >= tierConfig.MaxMembers;
                message = isExceeded
                    ? $"Member limit reached ({tierConfig.MaxMembers}). Upgrade to add more members."
                    : null;
                break;

            case "storage":
                isExceeded = usage is not null && usage.CurrentStorageBytes >= tierConfig.MaxStorageBytes;
                message = isExceeded
                    ? $"Storage limit reached ({tierConfig.MaxStorageBytes / (1024 * 1024 * 1024)} GB). Upgrade for more storage."
                    : null;
                break;

            case "sms":
                isExceeded = usage is not null && usage.CurrentMonthlySmsUsed >= tierConfig.MonthlySmsCredits;
                message = isExceeded
                    ? $"SMS credit limit reached ({tierConfig.MonthlySmsCredits}). Upgrade for more SMS credits."
                    : null;
                break;

            default:
                return Result.Failure<UsageLimitCheckDto>($"Unknown limit type: {request.LimitType}. Valid types: members, storage, sms.");
        }

        var dto = new UsageLimitCheckDto(request.LimitType, isExceeded, message);
        return Result.Success(dto);
    }
}
