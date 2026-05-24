using TheLeague.Shared.Domain.Entities;

namespace TheLeague.Modules.Events.Domain;

public class EventRegistration : TenantEntity
{
    public Guid EventId { get; private set; }
    public Guid MemberId { get; private set; }
    public string RegistrationType { get; private set; } = string.Empty; // "Ticket" or "RSVP"
    public DateTime RegisteredAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public bool RefundInitiated { get; private set; }

    private EventRegistration() { }

    public static EventRegistration Create(
        Guid clubId,
        Guid eventId,
        Guid memberId,
        string registrationType)
    {
        return new EventRegistration
        {
            ClubId = clubId,
            EventId = eventId,
            MemberId = memberId,
            RegistrationType = registrationType,
            RegisteredAt = DateTime.UtcNow
        };
    }

    public void Cancel()
    {
        CancelledAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void InitiateRefund()
    {
        RefundInitiated = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
