using MediatR;
using Microsoft.AspNetCore.Mvc;
using TheLeague.Modules.Analytics.Application.Commands;
using TheLeague.Modules.Analytics.Application.Queries;
using TheLeague.Shared.Infrastructure.Authorization;

namespace TheLeague.Modules.Analytics.Api;

[ApiController]
[Route("api/v1/analytics")]
public class AnalyticsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AnalyticsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("health-score")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> GetHealthScore(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetHealthScoreQuery(), ct);
        return Ok(result);
    }

    [HttpGet("churn-predictions")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> GetChurnPredictions(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetChurnPredictionsQuery(), ct);
        return Ok(result);
    }

    [HttpGet("member/{memberId:guid}/engagement")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> GetMemberEngagement(Guid memberId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetMemberEngagementQuery(memberId), ct);
        return Ok(result);
    }

    [HttpGet("revenue-forecast")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> GetRevenueForecast(
        [FromQuery] int activeMembershipCount,
        [FromQuery] decimal averageMonthlyFee,
        [FromQuery] decimal historicalRenewalRate,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new GetRevenueForecastQuery(activeMembershipCount, averageMonthlyFee, historicalRenewalRate), ct);
        return Ok(result);
    }

    [HttpGet("benchmarking")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> GetBenchmarking(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetBenchmarkingQuery(), ct);
        return Ok(result);
    }

    [HttpGet("snapshots")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> GetSnapshots([FromQuery] int months = 24, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetSnapshotsQuery(months), ct);
        return Ok(result);
    }
}
