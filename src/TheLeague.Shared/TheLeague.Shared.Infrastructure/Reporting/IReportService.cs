namespace TheLeague.Shared.Infrastructure.Reporting;

public interface IReportService
{
    Task<MembershipReportResult> GenerateMembershipReportAsync(Guid clubId, DateTime fromDate, DateTime toDate, CancellationToken ct = default);
    Task<FinancialReportResult> GenerateFinancialReportAsync(Guid clubId, DateTime fromDate, DateTime toDate, CancellationToken ct = default);
    Task<AttendanceReportResult> GenerateAttendanceReportAsync(Guid clubId, DateTime fromDate, DateTime toDate, CancellationToken ct = default);
}

public record MembershipReportResult(
    int ActiveCount,
    int ExpiredCount,
    int PendingCount,
    Dictionary<string, int> ByType,
    decimal RetentionRate);

public record FinancialReportResult(
    decimal TotalRevenue,
    decimal Outstanding,
    Dictionary<string, decimal> ByMethod,
    Dictionary<string, decimal> ByType);

public record AttendanceReportResult(
    decimal UtilisationRate,
    decimal NoShowPercentage,
    int TotalSessions,
    int TotalBookings);
