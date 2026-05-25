using MediatR;
using TheLeague.Modules.Analytics.Application.Dtos;
using TheLeague.Modules.Analytics.Domain;
using TheLeague.Modules.Analytics.Infrastructure.Persistence;
using TheLeague.Modules.Analytics.Infrastructure.Services;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Analytics.Application.Commands;

public record CaptureSnapshotCommand(
    decimal MemberGrowthRate,
    decimal PaymentCollectionRate,
    decimal SessionAttendanceRate,
    decimal EventParticipationRate,
    int ActiveMemberCount,
    decimal TotalRevenue,
    int TotalSessions,
    int TotalEvents
) : IRequest<Result<SnapshotDto>>;

public class CaptureSnapshotCommandHandler : IRequestHandler<CaptureSnapshotCommand, Result<SnapshotDto>>
{
    private readonly AnalyticsDbContext _db;
    private readonly ITenantService _tenantService;
    private readonly IHealthScoreCalculator _healthScoreCalculator;

    public CaptureSnapshotCommandHandler(
        AnalyticsDbContext db,
        ITenantService tenantService,
        IHealthScoreCalculator healthScoreCalculator)
    {
        _db = db;
        _tenantService = tenantService;
        _healthScoreCalculator = healthScoreCalculator;
    }

    public async Task<Result<SnapshotDto>> Handle(CaptureSnapshotCommand request, CancellationToken cancellationToken)
    {
        if (_tenantService.CurrentTenantId is null)
            return Result.Failure<SnapshotDto>("Tenant context is required.");

        var healthScore = _healthScoreCalculator.Calculate(
            request.MemberGrowthRate,
            request.PaymentCollectionRate,
            request.SessionAttendanceRate,
            request.EventParticipationRate);

        var snapshot = ClubAnalyticsSnapshot.Create(
            _tenantService.CurrentTenantId.Value,
            DateTime.UtcNow,
            request.MemberGrowthRate,
            request.PaymentCollectionRate,
            request.SessionAttendanceRate,
            request.EventParticipationRate,
            healthScore,
            request.ActiveMemberCount,
            request.TotalRevenue,
            request.TotalSessions,
            request.TotalEvents);

        _db.Snapshots.Add(snapshot);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(new SnapshotDto(
            snapshot.Id,
            snapshot.SnapshotDate,
            snapshot.MemberGrowthRate,
            snapshot.PaymentCollectionRate,
            snapshot.SessionAttendanceRate,
            snapshot.EventParticipationRate,
            snapshot.HealthScore,
            snapshot.ActiveMemberCount,
            snapshot.TotalRevenue,
            snapshot.TotalSessions,
            snapshot.TotalEvents));
    }
}
