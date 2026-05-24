using TheLeague.Shared.Contracts.Messaging;
using TheLeague.Shared.Domain.Enums;

namespace TheLeague.Shared.Contracts.Events;

public record UserRegisteredEvent(
    Guid UserId,
    string Email,
    UserRole Role,
    Guid? ClubId) : IntegrationEvent;

public record UserRoleChangedEvent(
    Guid UserId,
    UserRole OldRole,
    UserRole NewRole) : IntegrationEvent;
