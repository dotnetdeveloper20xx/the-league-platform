using TheLeague.Shared.Domain.Entities;

namespace TheLeague.Modules.Competitions.Domain;

public class CompetitionTeam : TenantEntity
{
    public Guid CompetitionId { get; private set; }
    public string TeamName { get; private set; } = string.Empty;
    public Guid? CaptainMemberId { get; private set; }
    public Guid? HomeVenueId { get; private set; }
    public string? HomeVenueName { get; private set; }
    public string? TeamColor { get; private set; }
    public int SquadSize { get; private set; }

    // Navigation
    public ICollection<CompetitionParticipant> Participants { get; private set; } = new List<CompetitionParticipant>();

    private CompetitionTeam() { }

    public static CompetitionTeam Create(
        Guid clubId,
        Guid competitionId,
        string teamName,
        Guid? captainMemberId,
        Guid? homeVenueId,
        string? homeVenueName,
        string? teamColor,
        int squadSize)
    {
        if (teamName.Length > 100)
            throw new ArgumentException("Team name must be at most 100 characters.");

        if (squadSize < 11 || squadSize > 30)
            throw new ArgumentException("Squad size must be between 11 and 30.");

        return new CompetitionTeam
        {
            ClubId = clubId,
            CompetitionId = competitionId,
            TeamName = teamName,
            CaptainMemberId = captainMemberId,
            HomeVenueId = homeVenueId,
            HomeVenueName = homeVenueName,
            TeamColor = teamColor,
            SquadSize = squadSize
        };
    }

    public void UpdateCaptain(Guid captainMemberId)
    {
        CaptainMemberId = captainMemberId;
        UpdatedAt = DateTime.UtcNow;
    }
}
