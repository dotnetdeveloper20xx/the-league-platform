using TheLeague.Shared.Domain.Entities;

namespace TheLeague.Modules.Events.Domain;

public class EventSession : TenantEntity
{
    public Guid EventId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public DateTime StartDateTime { get; private set; }
    public DateTime EndDateTime { get; private set; }
    public string? VenueName { get; private set; }
    public int SessionOrder { get; private set; }

    private EventSession() { }

    public static EventSession Create(
        Guid clubId,
        Guid eventId,
        string title,
        DateTime startDateTime,
        DateTime endDateTime,
        string? venueName,
        int sessionOrder)
    {
        if (sessionOrder < 1 || sessionOrder > 20)
            throw new InvalidOperationException("Session order must be between 1 and 20.");

        return new EventSession
        {
            ClubId = clubId,
            EventId = eventId,
            Title = title,
            StartDateTime = startDateTime,
            EndDateTime = endDateTime,
            VenueName = venueName,
            SessionOrder = sessionOrder
        };
    }
}
