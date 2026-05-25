using TheLeague.Shared.Domain.Entities;

namespace TheLeague.Modules.Analytics.Domain;

public class MemberEngagement : TenantEntity
{
    public Guid MemberId { get; private set; }
    public DateOnly Month { get; private set; }
    public int SessionsAttended { get; private set; }
    public int EventsAttended { get; private set; }
    public decimal PaymentTimelinessDays { get; private set; }
    public int PortalLogins { get; private set; }

    private MemberEngagement() { }

    public static MemberEngagement Create(
        Guid clubId,
        Guid memberId,
        DateOnly month,
        int sessionsAttended,
        int eventsAttended,
        decimal paymentTimelinessDays,
        int portalLogins)
    {
        return new MemberEngagement
        {
            ClubId = clubId,
            MemberId = memberId,
            Month = month,
            SessionsAttended = sessionsAttended,
            EventsAttended = eventsAttended,
            PaymentTimelinessDays = paymentTimelinessDays,
            PortalLogins = portalLogins
        };
    }
}
