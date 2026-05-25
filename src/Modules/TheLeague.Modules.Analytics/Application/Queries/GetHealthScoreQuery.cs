using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Analytics.Application.Dtos;
using TheLeague.Modules.Analytics.Infrastructure.Persistence;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Analytics.Application.Queries;

public record GetHealthScoreQuery : IRequest<Result<HealthScoreDto>>;

public class GetHealthScoreQueryHandler : IRequestHandler<GetHealthScoreQuery, Result<HealthScoreDto>>
{
    private readonly AnalyticsDbContext _db;
    private readonly ITenantService _tenantService;

    public GetHealthScoreQueryHandler(AnalyticsDbContext db, ITenantService tenantService)
    {
        _db = db;
        _tenantService = tenantService;
    }

    public async Task<Result<HealthScoreDto>> Handle(GetHealthScoreQuery request, CancellationToken cancellationToken)
    {
        if (_tenantService.CurrentTenantId is null)
            return Result.Failure<HealthScoreDto>("Tenant context is required.");

        var latestSnapshot = await _db.Snapshots
            .OrderByDescending(s => s.SnapshotDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (latestSnapshot is null)
        {
            return Result.Success(new HealthScoreDto(0, 0, 0, 0, 0, 0, null));
        }

        return Result.Success(new HealthScoreDto(
            latestSnapshot.HealthScore,
            latestSnapshot.MemberGrowthRate,
            latestSnapshot.PaymentCollectionRate,
            latestSnapshot.SessionAttendanceRate,
            latestSnapshot.EventParticipationRate,
            latestSnapshot.ActiveMemberCount,
            latestSnapshot.SnapshotDate));
    }
}
