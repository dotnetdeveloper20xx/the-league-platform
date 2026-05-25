using TheLeague.Shared.Domain.Entities;

namespace TheLeague.Modules.Facilities.Domain;

public class FacilityBlockout : TenantEntity
{
    public Guid FacilityId { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }

    public Facility Facility { get; private set; } = null!;

    public static FacilityBlockout Create(Guid clubId, Guid facilityId, string reason, DateTime startDate, DateTime endDate)
    {
        return new FacilityBlockout
        {
            ClubId = clubId,
            FacilityId = facilityId,
            Reason = reason,
            StartDate = startDate,
            EndDate = endDate
        };
    }
}
