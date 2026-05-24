using TheLeague.Shared.Contracts.Messaging;

namespace TheLeague.Shared.Contracts.Events;

public record BookingConfirmedEvent(
    Guid BookingId,
    Guid SessionId,
    Guid MemberId,
    Guid ClubId) : IntegrationEvent;

public record BookingCancelledEvent(
    Guid BookingId,
    Guid SessionId,
    Guid MemberId,
    Guid ClubId) : IntegrationEvent;

public record SessionCancelledEvent(
    Guid SessionId,
    Guid ClubId,
    string Reason) : IntegrationEvent;
