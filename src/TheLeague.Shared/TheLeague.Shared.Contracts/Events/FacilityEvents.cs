using TheLeague.Shared.Contracts.Messaging;

namespace TheLeague.Shared.Contracts.Events;

public record FacilityBookedEvent(
    Guid BookingId,
    Guid FacilityId,
    Guid MemberId,
    Guid ClubId) : IntegrationEvent;
