using TheLeague.Shared.Domain.Entities;
using TheLeague.Shared.Domain.Enums;

namespace TheLeague.Modules.Sessions.Domain;

public class Session : TenantEntity
{
    public string Title { get; private set; } = string.Empty;
    public SessionCategory Category { get; private set; }
    public Guid? VenueId { get; private set; }
    public string? VenueName { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public int Duration { get; private set; } // minutes, 15-480
    public int Capacity { get; private set; } // 1-500
    public decimal Fee { get; private set; } // 0-9999.99
    public int CurrentBookingCount { get; private set; }
    public bool IsCancelled { get; private set; }
    public string? CancellationReason { get; private set; }
    public int CancellationDeadlineHours { get; private set; } = 24;

    // Navigation properties
    public ICollection<SessionBooking> Bookings { get; private set; } = new List<SessionBooking>();
    public ICollection<Waitlist> WaitlistEntries { get; private set; } = new List<Waitlist>();

    public static Session Create(
        Guid clubId,
        string title,
        SessionCategory category,
        Guid? venueId,
        string? venueName,
        DateTime startTime,
        int duration,
        int capacity,
        decimal fee,
        int cancellationDeadlineHours = 24)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Length > 100)
            throw new ArgumentException("Title must be between 1 and 100 characters.");
        if (duration < 15 || duration > 480)
            throw new ArgumentException("Duration must be between 15 and 480 minutes.");
        if (capacity < 1 || capacity > 500)
            throw new ArgumentException("Capacity must be between 1 and 500.");
        if (fee < 0 || fee > 9999.99m)
            throw new ArgumentException("Fee must be between 0 and 9999.99.");

        return new Session
        {
            ClubId = clubId,
            Title = title,
            Category = category,
            VenueId = venueId,
            VenueName = venueName,
            StartTime = startTime,
            EndTime = startTime.AddMinutes(duration),
            Duration = duration,
            Capacity = capacity,
            Fee = fee,
            CancellationDeadlineHours = cancellationDeadlineHours,
            CurrentBookingCount = 0,
            IsCancelled = false
        };
    }

    public bool CanBook() => !IsCancelled && CurrentBookingCount < Capacity;

    public bool IsWaitlistAvailable(int currentWaitlistCount) => !IsCancelled && CurrentBookingCount >= Capacity && currentWaitlistCount < 50;

    public void IncrementBookings()
    {
        if (CurrentBookingCount >= Capacity)
            throw new InvalidOperationException("Session is at full capacity.");
        CurrentBookingCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void DecrementBookings()
    {
        if (CurrentBookingCount <= 0)
            throw new InvalidOperationException("No bookings to decrement.");
        CurrentBookingCount--;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel(string reason)
    {
        if (IsCancelled)
            throw new InvalidOperationException("Session is already cancelled.");
        IsCancelled = true;
        CancellationReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool CanCancelBooking(DateTime now)
    {
        var deadline = StartTime.AddHours(-CancellationDeadlineHours);
        return now <= deadline;
    }
}
