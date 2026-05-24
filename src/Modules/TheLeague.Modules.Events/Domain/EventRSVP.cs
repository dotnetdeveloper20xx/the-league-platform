using TheLeague.Shared.Domain.Entities;
using TheLeague.Shared.Domain.Enums;

namespace TheLeague.Modules.Events.Domain;

public class EventRSVP : TenantEntity
{
    public Guid EventId { get; private set; }
    public Guid MemberId { get; private set; }
    public RSVPResponse Response { get; private set; }
    public int GuestCount { get; private set; }
    public DateTime RespondedAt { get; private set; }

    private EventRSVP() { }

    public static EventRSVP Create(
        Guid clubId,
        Guid eventId,
        Guid memberId,
        RSVPResponse response,
        int guestCount)
    {
        return new EventRSVP
        {
            ClubId = clubId,
            EventId = eventId,
            MemberId = memberId,
            Response = response,
            GuestCount = guestCount,
            RespondedAt = DateTime.UtcNow
        };
    }

    public void UpdateResponse(RSVPResponse response, int guestCount)
    {
        Response = response;
        GuestCount = guestCount;
        RespondedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
