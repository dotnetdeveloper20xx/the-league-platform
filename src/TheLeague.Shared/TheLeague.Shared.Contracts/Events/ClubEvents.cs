using TheLeague.Shared.Contracts.Messaging;

namespace TheLeague.Shared.Contracts.Events;

public record ClubCreatedEvent(Guid ClubId, string Name, string Slug) : IntegrationEvent;

public record ClubDeactivatedEvent(Guid ClubId) : IntegrationEvent;
