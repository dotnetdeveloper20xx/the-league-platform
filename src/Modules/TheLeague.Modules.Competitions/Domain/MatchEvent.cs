using TheLeague.Shared.Domain.Entities;

namespace TheLeague.Modules.Competitions.Domain;

public class MatchEvent : TenantEntity
{
    public Guid MatchId { get; private set; }
    public Guid TeamId { get; private set; }
    public Guid? PlayerId { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public int? Minute { get; private set; }
    public string? Description { get; private set; }
    public DateTime Timestamp { get; private set; } = DateTime.UtcNow;

    private MatchEvent() { }

    public static MatchEvent Create(
        Guid clubId,
        Guid matchId,
        Guid teamId,
        Guid? playerId,
        string eventType,
        int? minute = null,
        string? description = null)
    {
        return new MatchEvent
        {
            ClubId = clubId,
            MatchId = matchId,
            TeamId = teamId,
            PlayerId = playerId,
            EventType = eventType,
            Minute = minute,
            Description = description,
            Timestamp = DateTime.UtcNow
        };
    }
}
