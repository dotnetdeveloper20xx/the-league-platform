using TheLeague.Shared.Domain.Enums;

namespace TheLeague.Modules.Competitions.Application.Dtos;

public record SeasonDto(
    Guid Id,
    Guid ClubId,
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive,
    DateTime CreatedAt);

public record CompetitionDto(
    Guid Id,
    Guid ClubId,
    Guid SeasonId,
    string Name,
    CompetitionType CompetitionType,
    string Status,
    int PointsForWin,
    int PointsForDraw,
    int PointsForLoss,
    string? DefaultWalkoverScore,
    DateTime CreatedAt);

public record CompetitionDetailDto(
    Guid Id,
    Guid ClubId,
    Guid SeasonId,
    string Name,
    CompetitionType CompetitionType,
    string Status,
    int PointsForWin,
    int PointsForDraw,
    int PointsForLoss,
    string? DefaultWalkoverScore,
    int TeamCount,
    int MatchCount,
    DateTime CreatedAt);

public record CompetitionTeamDto(
    Guid Id,
    Guid CompetitionId,
    string TeamName,
    Guid? CaptainMemberId,
    Guid? HomeVenueId,
    string? HomeVenueName,
    string? TeamColor,
    int SquadSize);

public record MatchDto(
    Guid Id,
    Guid CompetitionId,
    int? RoundNumber,
    Guid HomeTeamId,
    Guid AwayTeamId,
    Guid? VenueId,
    string? VenueName,
    DateTime ScheduledDateTime,
    MatchStatus Status,
    string? HomeScore,
    string? AwayScore,
    MatchResult Result);

public record MatchDetailDto(
    Guid Id,
    Guid CompetitionId,
    int? RoundNumber,
    Guid HomeTeamId,
    Guid AwayTeamId,
    Guid? VenueId,
    string? VenueName,
    DateTime ScheduledDateTime,
    MatchStatus Status,
    string? HomeScore,
    string? AwayScore,
    MatchResult Result,
    List<MatchEventDto> Events,
    List<MatchLineupDto> Lineups);

public record MatchEventDto(
    Guid Id,
    Guid MatchId,
    Guid TeamId,
    Guid? PlayerId,
    string EventType,
    int? Minute,
    string? Description,
    DateTime Timestamp);

public record MatchLineupDto(
    Guid Id,
    Guid MatchId,
    Guid TeamId,
    Guid PlayerId,
    bool IsStarter,
    string? Position);

public record CompetitionStandingDto(
    Guid Id,
    Guid CompetitionId,
    Guid TeamId,
    int Played,
    int Won,
    int Drawn,
    int Lost,
    int GoalsFor,
    int GoalsAgainst,
    int GoalDifference,
    int Points,
    string? Form);
