using TheLeague.Shared.Domain.Entities;

namespace TheLeague.Modules.Analytics.Domain;

public class ClubAnalyticsSnapshot : TenantEntity
{
    public DateTime SnapshotDate { get; private set; }
    public decimal MemberGrowthRate { get; private set; }
    public decimal PaymentCollectionRate { get; private set; }
    public decimal SessionAttendanceRate { get; private set; }
    public decimal EventParticipationRate { get; private set; }
    public int HealthScore { get; private set; }
    public int ActiveMemberCount { get; private set; }
    public decimal TotalRevenue { get; private set; }
    public int TotalSessions { get; private set; }
    public int TotalEvents { get; private set; }

    private ClubAnalyticsSnapshot() { }

    public static ClubAnalyticsSnapshot Create(
        Guid clubId,
        DateTime snapshotDate,
        decimal memberGrowthRate,
        decimal paymentCollectionRate,
        decimal sessionAttendanceRate,
        decimal eventParticipationRate,
        int healthScore,
        int activeMemberCount,
        decimal totalRevenue,
        int totalSessions,
        int totalEvents)
    {
        if (healthScore < 0) healthScore = 0;
        if (healthScore > 100) healthScore = 100;

        return new ClubAnalyticsSnapshot
        {
            ClubId = clubId,
            SnapshotDate = snapshotDate,
            MemberGrowthRate = memberGrowthRate,
            PaymentCollectionRate = paymentCollectionRate,
            SessionAttendanceRate = sessionAttendanceRate,
            EventParticipationRate = eventParticipationRate,
            HealthScore = healthScore,
            ActiveMemberCount = activeMemberCount,
            TotalRevenue = totalRevenue,
            TotalSessions = totalSessions,
            TotalEvents = totalEvents
        };
    }
}
