using Microsoft.Extensions.Logging;

namespace TheLeague.Shared.Infrastructure.Reporting;

public class ReportService : IReportService
{
    private readonly ILogger<ReportService> _logger;

    public ReportService(ILogger<ReportService> logger)
    {
        _logger = logger;
    }

    public Task<MembershipReportResult> GenerateMembershipReportAsync(Guid clubId, DateTime fromDate, DateTime toDate, CancellationToken ct = default)
    {
        _logger.LogInformation("Generating membership report for club {ClubId} from {From} to {To}", clubId, fromDate, toDate);

        var result = new MembershipReportResult(
            ActiveCount: 0,
            ExpiredCount: 0,
            PendingCount: 0,
            ByType: new Dictionary<string, int>(),
            RetentionRate: 0m);

        return Task.FromResult(result);
    }

    public Task<FinancialReportResult> GenerateFinancialReportAsync(Guid clubId, DateTime fromDate, DateTime toDate, CancellationToken ct = default)
    {
        _logger.LogInformation("Generating financial report for club {ClubId} from {From} to {To}", clubId, fromDate, toDate);

        var result = new FinancialReportResult(
            TotalRevenue: 0m,
            Outstanding: 0m,
            ByMethod: new Dictionary<string, decimal>(),
            ByType: new Dictionary<string, decimal>());

        return Task.FromResult(result);
    }

    public Task<AttendanceReportResult> GenerateAttendanceReportAsync(Guid clubId, DateTime fromDate, DateTime toDate, CancellationToken ct = default)
    {
        _logger.LogInformation("Generating attendance report for club {ClubId} from {From} to {To}", clubId, fromDate, toDate);

        var result = new AttendanceReportResult(
            UtilisationRate: 0m,
            NoShowPercentage: 0m,
            TotalSessions: 0,
            TotalBookings: 0);

        return Task.FromResult(result);
    }
}
