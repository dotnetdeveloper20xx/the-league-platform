using MediatR;
using Microsoft.AspNetCore.Mvc;
using TheLeague.Modules.Competitions.Application.Commands;
using TheLeague.Modules.Competitions.Application.Queries;
using TheLeague.Shared.Domain.Enums;
using TheLeague.Shared.Infrastructure.Authorization;

namespace TheLeague.Modules.Competitions.Api;

[ApiController]
[Route("api/v1")]
public class CompetitionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CompetitionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // Seasons

    [HttpGet("seasons")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> GetSeasons(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSeasonsQuery(), ct);
        return Ok(result);
    }

    [HttpPost("seasons")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> CreateSeason([FromBody] CreateSeasonCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetSeasons), result);
    }

    // Competitions

    [HttpGet("competitions")]
    [RequireRole(Roles.ClubManager, Roles.Member)]
    public async Task<IActionResult> GetCompetitions([FromQuery] Guid? seasonId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCompetitionsQuery(seasonId), ct);
        return Ok(result);
    }

    [HttpPost("competitions")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> CreateCompetition([FromBody] CreateCompetitionCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetCompetitionById), new { id = result.Data!.Id }, result);
    }

    [HttpGet("competitions/{id:guid}")]
    [RequireRole(Roles.ClubManager, Roles.Member)]
    public async Task<IActionResult> GetCompetitionById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCompetitionByIdQuery(id), ct);
        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPost("competitions/{id:guid}/teams")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> RegisterTeam(Guid id, [FromBody] RegisterTeamRequest request, CancellationToken ct)
    {
        var command = new RegisterTeamCommand(
            id, request.TeamName, request.CaptainMemberId,
            request.HomeVenueId, request.HomeVenueName,
            request.TeamColor, request.Players);

        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetCompetitionById), new { id }, result);
    }

    [HttpPost("competitions/{id:guid}/fixtures/generate")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> GenerateFixtures(Guid id, [FromBody] GenerateFixturesRequest request, CancellationToken ct)
    {
        var command = new GenerateFixturesCommand(id, request.StartDate);
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("competitions/{id:guid}/fixtures")]
    [RequireRole(Roles.ClubManager, Roles.Member)]
    public async Task<IActionResult> GetFixtures(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetFixturesQuery(id), ct);
        return Ok(result);
    }

    [HttpGet("competitions/{id:guid}/standings")]
    [RequireRole(Roles.ClubManager, Roles.Member)]
    public async Task<IActionResult> GetStandings(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetStandingsQuery(id), ct);
        return Ok(result);
    }

    // Matches

    [HttpGet("matches/{id:guid}")]
    [RequireRole(Roles.ClubManager, Roles.Member)]
    public async Task<IActionResult> GetMatchById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetMatchByIdQuery(id), ct);
        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPost("matches/{id:guid}/result")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> RecordResult(Guid id, [FromBody] RecordResultRequest request, CancellationToken ct)
    {
        var command = new RecordMatchResultCommand(id, request.HomeScore, request.AwayScore, request.Result);
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("matches/{id:guid}/events")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> RecordEvent(Guid id, [FromBody] RecordEventRequest request, CancellationToken ct)
    {
        var command = new RecordMatchEventCommand(
            id, request.TeamId, request.PlayerId,
            request.EventType, request.Minute, request.Description);

        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetMatchById), new { id }, result);
    }

    [HttpPost("matches/{id:guid}/status")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request, CancellationToken ct)
    {
        var command = new UpdateMatchStatusCommand(id, request.NewStatus, request.WinningTeamId);
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }
}

// Request DTOs

public record RegisterTeamRequest(
    string TeamName,
    Guid? CaptainMemberId,
    Guid? HomeVenueId,
    string? HomeVenueName,
    string? TeamColor,
    List<TeamParticipantRequest> Players
);

public record GenerateFixturesRequest(DateTime StartDate);

public record RecordResultRequest(
    string HomeScore,
    string AwayScore,
    MatchResult Result
);

public record RecordEventRequest(
    Guid TeamId,
    Guid? PlayerId,
    string EventType,
    int? Minute,
    string? Description
);

public record UpdateStatusRequest(
    MatchStatus NewStatus,
    Guid? WinningTeamId = null
);
