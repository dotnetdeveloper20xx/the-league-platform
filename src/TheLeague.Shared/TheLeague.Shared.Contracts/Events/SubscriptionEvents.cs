using TheLeague.Shared.Contracts.Messaging;
using TheLeague.Shared.Domain.Enums;

namespace TheLeague.Shared.Contracts.Events;

public record SubscriptionChangedEvent(
    Guid ClubId,
    SubscriptionTier OldTier,
    SubscriptionTier NewTier) : IntegrationEvent;

public record TrialExpiredEvent(Guid ClubId) : IntegrationEvent;

public record LimitReachedEvent(Guid ClubId, string LimitType) : IntegrationEvent;
