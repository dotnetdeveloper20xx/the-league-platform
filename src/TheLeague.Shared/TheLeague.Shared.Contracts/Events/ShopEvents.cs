using TheLeague.Shared.Contracts.Messaging;

namespace TheLeague.Shared.Contracts.Events;

public record OrderCreatedEvent(
    Guid OrderId,
    Guid MemberId,
    Guid ClubId) : IntegrationEvent;
