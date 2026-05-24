using TheLeague.Shared.Domain.Entities;
using TheLeague.Shared.Domain.Enums;

namespace TheLeague.Modules.Competitions.Domain;

public class Competition : TenantEntity
{
    public Guid SeasonId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public CompetitionType CompetitionType { get; private set; }
    public string Status { get; private set; } = "Draft";
    public int PointsForWin { get; private set; } = 3;
    public int PointsForDraw { get; private set; } = 1;
    public int PointsForLoss { get; private set; } = 0;
    public string? DefaultWalkoverScore { get; private set; }

    // Navigation
    public ICollection<CompetitionTeam> Teams { get; private set; } = new List<CompetitionTeam>();
    public ICollection<Match> Matches { get; private set; } = new List<Match>();
    public ICollection<CompetitionStanding> Standings { get; private set; } = new List<CompetitionStanding>();

    private Competition() { }

    public static Competition Create(
        Guid clubId,
        Guid seasonId,
        string name,
        CompetitionType competitionType,
        int pointsForWin = 3,
        int pointsForDraw = 1,
        int pointsForLoss = 0,
        string? defaultWalkoverScore = null)
    {
        if (name.Length > 200)
            throw new ArgumentException("Competition name must be at most 200 characters.");

        return new Competition
        {
            ClubId = clubId,
            SeasonId = seasonId,
            Name = name,
            CompetitionType = competitionType,
            PointsForWin = pointsForWin,
            PointsForDraw = pointsForDraw,
            PointsForLoss = pointsForLoss,
            DefaultWalkoverScore = defaultWalkoverScore,
            Status = "Draft"
        };
    }

    public void Activate()
    {
        Status = "Active";
        UpdatedAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        Status = "Completed";
        UpdatedAt = DateTime.UtcNow;
    }
}
