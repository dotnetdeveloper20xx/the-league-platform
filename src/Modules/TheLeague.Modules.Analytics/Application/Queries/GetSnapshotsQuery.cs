using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Analytics.Application.Dtos;
using TheLeague.Modules.Analytics.Infrastructure.Persistence;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Analytics.Application.Queries;

public record GetSnapshotsQuery(int Months = 24) : IRequest<Result<List<SnapshotDto>>>;

public class GetSnapshotsQueryHandler : IRequestHandler<GetSnapshotsQuery, Result<List<SnapshotDto>>>
{
    private readonly AnalyticsDbContext _db;
    private readonly ITenantService _tenantService;

    public GetSnapshotsQueryHandler(AnalyticsDbContext db, ITenantService tenantService)
    {
        _db = db;
        _tenantService = tenantService;
    }

    public async Task<Result<List<SnapshotDto>>> Handle(GetSnapshotsQuery request, CancellationToken cancellationToken)
    {
        if (_tenantService.CurrentTenantId is null)
            return Result.Failure<List<SnapshotDto>>("Tenant context is required.");

        var cutoffDate = DateTime.UtcNow.AddMonths(-request.Months);

        var snapshots = await _db.Snapshots
            .Where(s => s.SnapshotDate >= cutoffDate)
            .OrderByDescending(s => s.SnapshotDate)
            .ToListAsync(cancellationToken);

        var dtos = snapshots.Select(s => new SnapshotDto(
            s.Id,
            s.SnapshotDate,
            s.MemberGrowthRate,
            s.PaymentCollectionRate,
            s.SessionAttendanceRate,
            s.EventParticipationRate,
            s.HealthScore,
            s.ActiveMemberCount,
            s.TotalRevenue,
            s.TotalSessions,
            s.TotalEvents)).ToList();

        return Result.Success(dtos);
    }
}
