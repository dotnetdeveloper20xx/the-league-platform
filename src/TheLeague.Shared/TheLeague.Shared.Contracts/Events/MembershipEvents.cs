using TheLeague.Shared.Contracts.Messaging;

namespace TheLeague.Shared.Contracts.Events;

public record MembershipEnrolledEvent(
    Guid MembershipId,
    Guid MemberId,
    Guid ClubId,
    Guid MembershipTypeId) : IntegrationEvent;

public record MembershipExpiredEvent(
    Guid MembershipId,
    Guid MemberId,
    Guid ClubId) : IntegrationEvent;

public record MembershipRenewedEvent(
    Guid MembershipId,
    Guid MemberId,
    Guid ClubId) : IntegrationEvent;
