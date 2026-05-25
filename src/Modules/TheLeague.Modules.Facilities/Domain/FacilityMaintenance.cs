using TheLeague.Shared.Domain.Entities;

namespace TheLeague.Modules.Facilities.Domain;

public class FacilityMaintenance : TenantEntity
{
    public Guid FacilityId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public bool IsCompleted { get; private set; }

    public Facility Facility { get; private set; } = null!;

    public static FacilityMaintenance Create(
        Guid clubId,
        Guid facilityId,
        string title,
        string? description,
        DateTime startDate,
        DateTime endDate)
    {
        return new FacilityMaintenance
        {
            ClubId = clubId,
            FacilityId = facilityId,
            Title = title,
            Description = description,
            StartDate = startDate,
            EndDate = endDate
        };
    }

    public void MarkCompleted()
    {
        IsCompleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
