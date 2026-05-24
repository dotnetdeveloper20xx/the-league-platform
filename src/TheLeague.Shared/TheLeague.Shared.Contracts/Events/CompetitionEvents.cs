using TheLeague.Shared.Contracts.Messaging;

namespace TheLeague.Shared.Contracts.Events;

public record MatchCompletedEvent(
    Guid MatchId,
    Guid CompetitionId,
    Guid ClubId) : IntegrationEvent;

public record StandingsUpdatedEvent(Guid CompetitionId, Guid ClubId) : IntegrationEvent;
