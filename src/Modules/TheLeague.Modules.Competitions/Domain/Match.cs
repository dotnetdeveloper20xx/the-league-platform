using TheLeague.Shared.Domain.Entities;
using TheLeague.Shared.Domain.Enums;

namespace TheLeague.Modules.Competitions.Domain;

public class Match : TenantEntity
{
    public Guid CompetitionId { get; private set; }
    public int? RoundNumber { get; private set; }
    public Guid HomeTeamId { get; private set; }
    public Guid AwayTeamId { get; private set; }
    public Guid? VenueId { get; private set; }
    public string? VenueName { get; private set; }
    public DateTime ScheduledDateTime { get; private set; }
    public MatchStatus Status { get; private set; } = MatchStatus.Scheduled;
    public string? HomeScore { get; private set; }
    public string? AwayScore { get; private set; }
    public MatchResult Result { get; private set; } = MatchResult.NotPlayed;

    // Navigation
    public ICollection<MatchEvent> Events { get; private set; } = new List<MatchEvent>();
    public ICollection<MatchLineup> Lineups { get; private set; } = new List<MatchLineup>();

    private Match() { }

    public static Match Create(
        Guid clubId,
        Guid competitionId,
        Guid homeTeamId,
        Guid awayTeamId,
        DateTime scheduledDateTime,
        int? roundNumber = null,
        Guid? venueId = null,
        string? venueName = null)
    {
        return new Match
        {
            ClubId = clubId,
            CompetitionId = competitionId,
            HomeTeamId = homeTeamId,
            AwayTeamId = awayTeamId,
            ScheduledDateTime = scheduledDateTime,
            RoundNumber = roundNumber,
            VenueId = venueId,
            VenueName = venueName,
            Status = MatchStatus.Scheduled,
            Result = MatchResult.NotPlayed
        };
    }

    public void Confirm()
    {
        ValidateTransition(MatchStatus.Confirmed);
        Status = MatchStatus.Confirmed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Start()
    {
        ValidateTransition(MatchStatus.InProgress);
        Status = MatchStatus.InProgress;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Complete(string homeScore, string awayScore, MatchResult result)
    {
        ValidateTransition(MatchStatus.Completed);
        Status = MatchStatus.Completed;
        HomeScore = homeScore;
        AwayScore = awayScore;
        Result = result;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Postpone()
    {
        ValidateTransition(MatchStatus.Postponed);
        Status = MatchStatus.Postponed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        ValidateTransition(MatchStatus.Cancelled);
        Status = MatchStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Abandon()
    {
        ValidateTransition(MatchStatus.Abandoned);
        Status = MatchStatus.Abandoned;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Walkover(Guid winningTeamId)
    {
        ValidateTransition(MatchStatus.Walkover);
        Status = MatchStatus.Walkover;

        if (winningTeamId == HomeTeamId)
            Result = MatchResult.HomeWalkover;
        else if (winningTeamId == AwayTeamId)
            Result = MatchResult.AwayWalkover;
        else
            throw new InvalidOperationException("Winning team must be one of the match participants.");

        UpdatedAt = DateTime.UtcNow;
    }

    private void ValidateTransition(MatchStatus newStatus)
    {
        var validTransitions = GetValidTransitions(Status);
        if (!validTransitions.Contains(newStatus))
        {
            throw new InvalidOperationException(
                $"Cannot transition match from '{Status}' to '{newStatus}'. " +
                $"Valid transitions from '{Status}' are: {string.Join(", ", validTransitions)}.");
        }
    }

    public static IReadOnlyList<MatchStatus> GetValidTransitions(MatchStatus currentStatus)
    {
        return currentStatus switch
        {
            MatchStatus.Scheduled => new[] { MatchStatus.Confirmed, MatchStatus.Cancelled },
            MatchStatus.Confirmed => new[] { MatchStatus.InProgress, MatchStatus.Postponed, MatchStatus.Cancelled },
            MatchStatus.InProgress => new[] { MatchStatus.Completed, MatchStatus.Abandoned },
            MatchStatus.Postponed => new[] { MatchStatus.Confirmed, MatchStatus.Cancelled },
            _ => Array.Empty<MatchStatus>()
        };
    }
}
