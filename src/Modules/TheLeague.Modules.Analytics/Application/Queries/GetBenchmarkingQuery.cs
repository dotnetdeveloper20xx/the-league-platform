using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Analytics.Application.Dtos;
using TheLeague.Modules.Analytics.Infrastructure.Persistence;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Analytics.Application.Queries;

public record GetBenchmarkingQuery : IRequest<Result<BenchmarkingDto>>;

public class GetBenchmarkingQueryHandler : IRequestHandler<GetBenchmarkingQuery, Result<BenchmarkingDto>>
{
    private readonly AnalyticsDbContext _db;
    private readonly ITenantService _tenantService;

    public GetBenchmarkingQueryHandler(AnalyticsDbContext db, ITenantService tenantService)
    {
        _db = db;
        _tenantService = tenantService;
    }

    public async Task<Result<BenchmarkingDto>> Handle(GetBenchmarkingQuery request, CancellationToken cancellationToken)
    {
        if (_tenantService.CurrentTenantId is null)
            return Result.Failure<BenchmarkingDto>("Tenant context is required.");

        // Get the latest snapshot for the current club
        var clubSnapshot = await _db.Snapshots
            .OrderByDescending(s => s.SnapshotDate)
            .FirstOrDefaultAsync(cancellationToken);

        // Get platform averages (ignoring tenant filter for aggregation)
        // In a real implementation, this would use a separate service or bypass tenant filters
        // For now, we use the club's own data as a baseline
        var platformSnapshots = await _db.Snapshots
            .IgnoreQueryFilters()
            .GroupBy(s => s.ClubId)
            .Select(g => g.OrderByDescending(s => s.SnapshotDate).First())
            .ToListAsync(cancellationToken);

        var platformAvgHealth = platformSnapshots.Count > 0
            ? (decimal)platformSnapshots.Average(s => s.HealthScore)
            : 0m;
        var platformAvgGrowth = platformSnapshots.Count > 0
            ? platformSnapshots.Average(s => s.MemberGrowthRate)
            : 0m;
        var platformAvgPayment = platformSnapshots.Count > 0
            ? platformSnapshots.Average(s => s.PaymentCollectionRate)
            : 0m;
        var platformAvgAttendance = platformSnapshots.Count > 0
            ? platformSnapshots.Average(s => s.SessionAttendanceRate)
            : 0m;
        var platformAvgEvents = platformSnapshots.Count > 0
            ? platformSnapshots.Average(s => s.EventParticipationRate)
            : 0m;

        return Result.Success(new BenchmarkingDto(
            clubSnapshot?.HealthScore ?? 0,
            platformAvgHealth,
            clubSnapshot?.MemberGrowthRate ?? 0,
            platformAvgGrowth,
            clubSnapshot?.PaymentCollectionRate ?? 0,
            platformAvgPayment,
            clubSnapshot?.SessionAttendanceRate ?? 0,
            platformAvgAttendance,
            clubSnapshot?.EventParticipationRate ?? 0,
            platformAvgEvents));
    }
}
