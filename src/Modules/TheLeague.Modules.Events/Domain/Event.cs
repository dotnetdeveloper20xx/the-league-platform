using TheLeague.Shared.Domain.Entities;
using TheLeague.Shared.Domain.Enums;

namespace TheLeague.Modules.Events.Domain;

public class Event : TenantEntity
{
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public EventType EventType { get; private set; }
    public EventStatus Status { get; private set; }
    public DateTime StartDateTime { get; private set; }
    public DateTime EndDateTime { get; private set; }
    public Guid? VenueId { get; private set; }
    public string? VenueName { get; private set; }
    public int? Capacity { get; private set; }
    public int CurrentRegistrationCount { get; private set; }
    public bool IsTicketed { get; private set; }
    public decimal? StandardPrice { get; private set; }
    public decimal? MemberPrice { get; private set; }
    public bool AllowRsvp { get; private set; }
    public int CancellationDeadlineHours { get; private set; } = 48;

    private Event() { }

    public static Event Create(
        Guid clubId,
        string title,
        EventType eventType,
        DateTime startDateTime,
        DateTime endDateTime)
    {
        if (endDateTime <= startDateTime)
            throw new InvalidOperationException("End date-time must be after start date-time.");

        if (string.IsNullOrWhiteSpace(title) || title.Length > 200)
            throw new InvalidOperationException("Title must be between 1 and 200 characters.");

        return new Event
        {
            ClubId = clubId,
            Title = title,
            EventType = eventType,
            Status = EventStatus.Draft,
            StartDateTime = startDateTime,
            EndDateTime = endDateTime
        };
    }

    public void Update(
        string title,
        string? description,
        EventType eventType,
        DateTime startDateTime,
        DateTime endDateTime,
        Guid? venueId,
        string? venueName,
        int? capacity,
        bool isTicketed,
        decimal? standardPrice,
        decimal? memberPrice,
        bool allowRsvp,
        int cancellationDeadlineHours)
    {
        if (Status != EventStatus.Draft)
            throw new InvalidOperationException("Can only update events in Draft status.");

        if (endDateTime <= startDateTime)
            throw new InvalidOperationException("End date-time must be after start date-time.");

        if (string.IsNullOrWhiteSpace(title) || title.Length > 200)
            throw new InvalidOperationException("Title must be between 1 and 200 characters.");

        Title = title;
        Description = description;
        EventType = eventType;
        StartDateTime = startDateTime;
        EndDateTime = endDateTime;
        VenueId = venueId;
        VenueName = venueName;
        Capacity = capacity;
        IsTicketed = isTicketed;
        StandardPrice = standardPrice;
        MemberPrice = memberPrice;
        AllowRsvp = allowRsvp;
        CancellationDeadlineHours = cancellationDeadlineHours;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Publish()
    {
        EnsureValidTransition(EventStatus.Published);
        Status = EventStatus.Published;
        UpdatedAt = DateTime.UtcNow;
    }

    public void OpenRegistration()
    {
        EnsureValidTransition(EventStatus.RegistrationOpen);
        Status = EventStatus.RegistrationOpen;
        UpdatedAt = DateTime.UtcNow;
    }

    public void CloseRegistration()
    {
        EnsureValidTransition(EventStatus.RegistrationClosed);
        Status = EventStatus.RegistrationClosed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Start()
    {
        EnsureValidTransition(EventStatus.InProgress);
        Status = EventStatus.InProgress;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        EnsureValidTransition(EventStatus.Completed);
        Status = EventStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        EnsureValidTransition(EventStatus.Cancelled);
        Status = EventStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Postpone()
    {
        EnsureValidTransition(EventStatus.Postponed);
        Status = EventStatus.Postponed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void IncrementRegistrationCount()
    {
        CurrentRegistrationCount++;
    }

    public void DecrementRegistrationCount()
    {
        if (CurrentRegistrationCount > 0)
            CurrentRegistrationCount--;
    }

    public bool IsAtCapacity()
    {
        return Capacity.HasValue && CurrentRegistrationCount >= Capacity.Value;
    }

    private void EnsureValidTransition(EventStatus newStatus)
    {
        var allowed = GetAllowedTransitions(Status);
        if (!allowed.Contains(newStatus))
        {
            throw new InvalidOperationException(
                $"Cannot transition from {Status} to {newStatus}. " +
                $"Allowed transitions from {Status}: {string.Join(", ", allowed)}");
        }
    }

    public static IReadOnlyList<EventStatus> GetAllowedTransitions(EventStatus currentStatus)
    {
        return currentStatus switch
        {
            EventStatus.Draft => new[] { EventStatus.Published, EventStatus.Cancelled },
            EventStatus.Published => new[] { EventStatus.RegistrationOpen, EventStatus.Cancelled, EventStatus.Postponed },
            EventStatus.RegistrationOpen => new[] { EventStatus.RegistrationClosed, EventStatus.Cancelled, EventStatus.Postponed },
            EventStatus.RegistrationClosed => new[] { EventStatus.InProgress, EventStatus.Cancelled, EventStatus.Postponed },
            EventStatus.InProgress => new[] { EventStatus.Completed, EventStatus.Cancelled, EventStatus.Postponed },
            EventStatus.Completed => Array.Empty<EventStatus>(),
            EventStatus.Cancelled => Array.Empty<EventStatus>(),
            EventStatus.Postponed => Array.Empty<EventStatus>(),
            _ => Array.Empty<EventStatus>()
        };
    }
}
