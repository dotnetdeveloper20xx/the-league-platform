using TheLeague.Shared.Contracts.Messaging;
using TheLeague.Shared.Domain.Enums;

namespace TheLeague.Shared.Contracts.Events;

public record MemberCreatedEvent(Guid MemberId, Guid ClubId, string Email) : IntegrationEvent;

public record MemberStatusChangedEvent(
    Guid MemberId,
    Guid ClubId,
    MemberStatus OldStatus,
    MemberStatus NewStatus) : IntegrationEvent;
