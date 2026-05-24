using TheLeague.Shared.Domain.Entities;

namespace TheLeague.Modules.Competitions.Domain;

public class Season : TenantEntity
{
    public string Name { get; private set; } = string.Empty;
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public bool IsActive { get; private set; }

    private Season() { }

    public static Season Create(Guid clubId, string name, DateTime startDate, DateTime endDate)
    {
        if (endDate <= startDate)
            throw new ArgumentException("End date must be after start date.");

        if (name.Length > 100)
            throw new ArgumentException("Season name must be at most 100 characters.");

        return new Season
        {
            ClubId = clubId,
            Name = name,
            StartDate = startDate,
            EndDate = endDate,
            IsActive = true
        };
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
