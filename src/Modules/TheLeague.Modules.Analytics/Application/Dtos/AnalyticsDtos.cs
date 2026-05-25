namespace TheLeague.Modules.Analytics.Application.Dtos;

public record HealthScoreDto(
    int HealthScore,
    decimal MemberGrowthRate,
    decimal PaymentCollectionRate,
    decimal SessionAttendanceRate,
    decimal EventParticipationRate,
    int ActiveMemberCount,
    DateTime? LastSnapshotDate
);

public record ChurnPredictionDto(
    Guid MemberId,
    DateTime PredictionDate,
    bool IsAtRisk,
    string? RiskFactors,
    decimal? AttendanceDropPercent,
    int? MissedPaymentCount,
    decimal? LoginDropPercent
);

public record MemberEngagementDto(
    Guid MemberId,
    DateOnly Month,
    int SessionsAttended,
    int EventsAttended,
    decimal PaymentTimelinessDays,
    int PortalLogins
);

public record RevenueForecastDto(
    List<MonthForecastDto> MonthlyForecasts,
    decimal TotalProjectedRevenue
);

public record MonthForecastDto(
    int MonthOffset,
    decimal ProjectedRevenue,
    decimal ProjectedMembershipRevenue,
    decimal ConfidenceLevel
);

public record BenchmarkingDto(
    decimal ClubHealthScore,
    decimal PlatformAverageHealthScore,
    decimal ClubMemberGrowthRate,
    decimal PlatformAverageMemberGrowthRate,
    decimal ClubPaymentCollectionRate,
    decimal PlatformAveragePaymentCollectionRate,
    decimal ClubSessionAttendanceRate,
    decimal PlatformAverageSessionAttendanceRate,
    decimal ClubEventParticipationRate,
    decimal PlatformAverageEventParticipationRate
);

public record SnapshotDto(
    Guid Id,
    DateTime SnapshotDate,
    decimal MemberGrowthRate,
    decimal PaymentCollectionRate,
    decimal SessionAttendanceRate,
    decimal EventParticipationRate,
    int HealthScore,
    int ActiveMemberCount,
    decimal TotalRevenue,
    int TotalSessions,
    int TotalEvents
);
