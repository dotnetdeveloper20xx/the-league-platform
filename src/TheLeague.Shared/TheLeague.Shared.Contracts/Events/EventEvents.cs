using TheLeague.Shared.Contracts.Messaging;

namespace TheLeague.Shared.Contracts.Events;

public record EventPublishedEvent(
    Guid EventId,
    Guid ClubId,
    string Title) : IntegrationEvent;

public record EventCancelledEvent(Guid EventId, Guid ClubId) : IntegrationEvent;

public record TicketPurchasedEvent(
    Guid TicketId,
    Guid EventId,
    Guid MemberId,
    Guid ClubId) : IntegrationEvent;
