using TheLeague.Shared.Domain.Entities;

namespace TheLeague.Modules.Facilities.Domain;

public class Facility : TenantEntity
{
    public string Name { get; private set; } = string.Empty;
    public FacilityType FacilityType { get; private set; }
    public string? Description { get; private set; }
    public int? Capacity { get; private set; }
    public bool IsActive { get; private set; } = true;

    public ICollection<FacilityBooking> Bookings { get; private set; } = new List<FacilityBooking>();
    public ICollection<FacilityAvailability> Availabilities { get; private set; } = new List<FacilityAvailability>();
    public ICollection<FacilityPricing> Pricings { get; private set; } = new List<FacilityPricing>();
    public ICollection<FacilityMaintenance> MaintenanceWindows { get; private set; } = new List<FacilityMaintenance>();
    public ICollection<FacilityBlockout> Blockouts { get; private set; } = new List<FacilityBlockout>();

    public static Facility Create(Guid clubId, string name, FacilityType facilityType, string? description, int? capacity)
    {
        return new Facility
        {
            ClubId = clubId,
            Name = name,
            FacilityType = facilityType,
            Description = description,
            Capacity = capacity
        };
    }

    public void Update(string name, FacilityType facilityType, string? description, int? capacity, bool isActive)
    {
        Name = name;
        FacilityType = facilityType;
        Description = description;
        Capacity = capacity;
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }
}
