using TheLeague.Shared.Domain.Entities;
using TheLeague.Shared.Domain.Enums;

namespace TheLeague.Modules.Events.Domain;

public class EventSeries : TenantEntity
{
    public string Title { get; private set; } = string.Empty;
    public EventType EventType { get; private set; }
    public int MaxOccurrences { get; private set; }
    public string? RecurrencePattern { get; private set; } // JSON
    public bool IsActive { get; private set; } = true;

    private EventSeries() { }

    public static EventSeries Create(Guid clubId, string title, EventType eventType, int maxOccurrences, string? recurrencePattern)
    {
        if (maxOccurrences < 1 || maxOccurrences > 52)
            throw new InvalidOperationException("Max occurrences must be between 1 and 52.");

        return new EventSeries
        {
            ClubId = clubId,
            Title = title,
            EventType = eventType,
            MaxOccurrences = maxOccurrences,
            RecurrencePattern = recurrencePattern
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
