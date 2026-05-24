using TheLeague.Shared.Domain.Entities;

namespace TheLeague.Modules.Competitions.Domain;

public class CompetitionStanding : TenantEntity
{
    public Guid CompetitionId { get; private set; }
    public Guid TeamId { get; private set; }
    public int Played { get; private set; }
    public int Won { get; private set; }
    public int Drawn { get; private set; }
    public int Lost { get; private set; }
    public int GoalsFor { get; private set; }
    public int GoalsAgainst { get; private set; }
    public int GoalDifference { get; private set; }
    public int Points { get; private set; }
    public string? Form { get; private set; }

    private CompetitionStanding() { }

    public static CompetitionStanding Create(Guid clubId, Guid competitionId, Guid teamId)
    {
        return new CompetitionStanding
        {
            ClubId = clubId,
            CompetitionId = competitionId,
            TeamId = teamId
        };
    }

    public void Update(int played, int won, int drawn, int lost, int goalsFor, int goalsAgainst, int points, string? form)
    {
        Played = played;
        Won = won;
        Drawn = drawn;
        Lost = lost;
        GoalsFor = goalsFor;
        GoalsAgainst = goalsAgainst;
        GoalDifference = goalsFor - goalsAgainst;
        Points = points;
        Form = form;
        UpdatedAt = DateTime.UtcNow;
    }
}
