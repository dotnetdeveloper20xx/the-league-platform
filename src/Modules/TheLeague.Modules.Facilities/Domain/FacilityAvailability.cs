using TheLeague.Shared.Domain.Entities;

namespace TheLeague.Modules.Facilities.Domain;

public class FacilityAvailability : TenantEntity
{
    public Guid FacilityId { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }
    public TimeOnly OpenTime { get; private set; }
    public TimeOnly CloseTime { get; private set; }
    public bool IsActive { get; private set; } = true;

    public Facility Facility { get; private set; } = null!;

    public static FacilityAvailability Create(Guid clubId, Guid facilityId, DayOfWeek dayOfWeek, TimeOnly openTime, TimeOnly closeTime)
    {
        return new FacilityAvailability
        {
            ClubId = clubId,
            FacilityId = facilityId,
            DayOfWeek = dayOfWeek,
            OpenTime = openTime,
            CloseTime = closeTime
        };
    }
}
