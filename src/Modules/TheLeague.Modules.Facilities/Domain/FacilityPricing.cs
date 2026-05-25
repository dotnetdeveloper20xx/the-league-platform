using TheLeague.Shared.Domain.Entities;

namespace TheLeague.Modules.Facilities.Domain;

public class FacilityPricing : TenantEntity
{
    public Guid FacilityId { get; private set; }
    public bool IsPeakRate { get; private set; }
    public decimal MemberRate { get; private set; }
    public decimal NonMemberRate { get; private set; }
    public TimeOnly? PeakStartTime { get; private set; }
    public TimeOnly? PeakEndTime { get; private set; }

    public Facility Facility { get; private set; } = null!;

    public static FacilityPricing Create(
        Guid clubId,
        Guid facilityId,
        bool isPeakRate,
        decimal memberRate,
        decimal nonMemberRate,
        TimeOnly? peakStartTime,
        TimeOnly? peakEndTime)
    {
        return new FacilityPricing
        {
            ClubId = clubId,
            FacilityId = facilityId,
            IsPeakRate = isPeakRate,
            MemberRate = memberRate,
            NonMemberRate = nonMemberRate,
            PeakStartTime = peakStartTime,
            PeakEndTime = peakEndTime
        };
    }
}
