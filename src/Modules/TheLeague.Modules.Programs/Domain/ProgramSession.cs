using TheLeague.Shared.Domain.Entities;

namespace TheLeague.Modules.Programs.Domain;

public class ProgramSession : TenantEntity
{
    public Guid ProgramId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public Guid? InstructorId { get; private set; }
    public string? InstructorName { get; private set; }
    public DateTime StartDateTime { get; private set; }
    public DateTime EndDateTime { get; private set; }
    public Guid? VenueId { get; private set; }
    public string? VenueName { get; private set; }
    public int MaxCapacity { get; private set; }
    public int SessionOrder { get; private set; }

    // Navigation properties
    public Program Program { get; private set; } = null!;
    public ICollection<ProgramAttendance> AttendanceRecords { get; private set; } = new List<ProgramAttendance>();

    public static ProgramSession Create(
        Guid clubId,
        Guid programId,
        string title,
        Guid? instructorId,
        string? instructorName,
        DateTime startDateTime,
        DateTime endDateTime,
        Guid? venueId,
        string? venueName,
        int maxCapacity,
        int sessionOrder)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.");
        if (maxCapacity < 1 || maxCapacity > 200)
            throw new ArgumentException("Max capacity must be between 1 and 200.");
        if (endDateTime <= startDateTime)
            throw new ArgumentException("End date/time must be after start date/time.");

        return new ProgramSession
        {
            ClubId = clubId,
            ProgramId = programId,
            Title = title,
            InstructorId = instructorId,
            InstructorName = instructorName,
            StartDateTime = startDateTime,
            EndDateTime = endDateTime,
            VenueId = venueId,
            VenueName = venueName,
            MaxCapacity = maxCapacity,
            SessionOrder = sessionOrder
        };
    }
}
