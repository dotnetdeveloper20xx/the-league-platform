using TheLeague.Shared.Domain.Entities;
using TheLeague.Shared.Domain.Enums;

namespace TheLeague.Modules.Programs.Domain;

public class Program : TenantEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public ProgramType ProgramType { get; private set; }
    public SkillLevel SkillLevel { get; private set; }
    public int Capacity { get; private set; }
    public decimal Price { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Navigation properties
    public ICollection<ProgramSession> Sessions { get; private set; } = new List<ProgramSession>();
    public ICollection<ProgramEnrollment> Enrollments { get; private set; } = new List<ProgramEnrollment>();

    public static Program Create(
        Guid clubId,
        string name,
        string? description,
        ProgramType programType,
        SkillLevel skillLevel,
        int capacity,
        decimal price,
        DateTime startDate,
        DateTime endDate)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 200)
            throw new ArgumentException("Name must be between 1 and 200 characters.");
        if (capacity < 1)
            throw new ArgumentException("Capacity must be at least 1.");
        if (price < 0)
            throw new ArgumentException("Price cannot be negative.");
        if (endDate <= startDate)
            throw new ArgumentException("End date must be after start date.");

        return new Program
        {
            ClubId = clubId,
            Name = name,
            Description = description,
            ProgramType = programType,
            SkillLevel = skillLevel,
            Capacity = capacity,
            Price = price,
            StartDate = startDate,
            EndDate = endDate,
            IsActive = true
        };
    }

    public void Update(
        string name,
        string? description,
        ProgramType programType,
        SkillLevel skillLevel,
        int capacity,
        decimal price,
        DateTime startDate,
        DateTime endDate,
        bool isActive)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 200)
            throw new ArgumentException("Name must be between 1 and 200 characters.");
        if (capacity < 1)
            throw new ArgumentException("Capacity must be at least 1.");
        if (price < 0)
            throw new ArgumentException("Price cannot be negative.");
        if (endDate <= startDate)
            throw new ArgumentException("End date must be after start date.");

        Name = name;
        Description = description;
        ProgramType = programType;
        SkillLevel = skillLevel;
        Capacity = capacity;
        Price = price;
        StartDate = startDate;
        EndDate = endDate;
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool HasEnded(DateTime now) => now >= EndDate;

    public int GetCurrentEnrollmentCount(IEnumerable<ProgramEnrollment> enrollments)
        => enrollments.Count(e => e.Status == EnrollmentStatus.Confirmed || e.Status == EnrollmentStatus.Attended);
}
