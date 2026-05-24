using TheLeague.Shared.Domain.Entities;

namespace TheLeague.Modules.Competitions.Domain;

public class MatchLineup : TenantEntity
{
    public Guid MatchId { get; private set; }
    public Guid TeamId { get; private set; }
    public Guid PlayerId { get; private set; }
    public bool IsStarter { get; private set; }
    public string? Position { get; private set; }

    private MatchLineup() { }

    public static MatchLineup Create(
        Guid clubId,
        Guid matchId,
        Guid teamId,
        Guid playerId,
        bool isStarter,
        string? position = null)
    {
        return new MatchLineup
        {
            ClubId = clubId,
            MatchId = matchId,
            TeamId = teamId,
            PlayerId = playerId,
            IsStarter = isStarter,
            Position = position
        };
    }
}
