using TheLeague.Shared.Domain.Entities;
using TheLeague.Shared.Domain.Enums;

namespace TheLeague.Modules.Sessions.Domain;

public class RecurringSchedule : TenantEntity
{
    public string Title { get; private set; } = string.Empty;
    public SessionCategory Category { get; private set; }
    public Guid? VenueId { get; private set; }
    public string? VenueName { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public int Duration { get; private set; } // minutes
    public int Capacity { get; private set; }
    public decimal Fee { get; private set; }
    public int HorizonWeeks { get; private set; } // max 12
    public bool IsActive { get; private set; } = true;

    public static RecurringSchedule Create(
        Guid clubId,
        string title,
        SessionCategory category,
        Guid? venueId,
        string? venueName,
        DayOfWeek dayOfWeek,
        TimeOnly startTime,
        int duration,
        int capacity,
        decimal fee,
        int horizonWeeks)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Length > 100)
            throw new ArgumentException("Title must be between 1 and 100 characters.");
        if (duration < 15 || duration > 480)
            throw new ArgumentException("Duration must be between 15 and 480 minutes.");
        if (capacity < 1 || capacity > 500)
            throw new ArgumentException("Capacity must be between 1 and 500.");
        if (fee < 0 || fee > 9999.99m)
            throw new ArgumentException("Fee must be between 0 and 9999.99.");
        if (horizonWeeks < 1 || horizonWeeks > 12)
            throw new ArgumentException("Horizon must be between 1 and 12 weeks.");

        return new RecurringSchedule
        {
            ClubId = clubId,
            Title = title,
            Category = category,
            VenueId = venueId,
            VenueName = venueName,
            DayOfWeek = dayOfWeek,
            StartTime = startTime,
            Duration = duration,
            Capacity = capacity,
            Fee = fee,
            HorizonWeeks = horizonWeeks,
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

    public List<Session> GenerateSessions(DateTime fromDate)
    {
        var sessions = new List<Session>();
        var endDate = fromDate.AddDays(HorizonWeeks * 7);
        var current = fromDate;

        while (current <= endDate)
        {
            if (current.DayOfWeek == DayOfWeek)
            {
                var sessionStart = current.Date.Add(StartTime.ToTimeSpan());
                if (sessionStart > fromDate)
                {
                    var session = Session.Create(
                        ClubId,
                        Title,
                        Category,
                        VenueId,
                        VenueName,
                        sessionStart,
                        Duration,
                        Capacity,
                        Fee);
                    sessions.Add(session);
                }
            }
            current = current.AddDays(1);
        }

        return sessions;
    }
}
