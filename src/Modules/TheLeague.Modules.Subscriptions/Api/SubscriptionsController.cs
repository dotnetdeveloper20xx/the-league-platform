using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheLeague.Modules.Subscriptions.Application.Commands;
using TheLeague.Modules.Subscriptions.Application.Queries;
using TheLeague.Shared.Domain.Enums;
using TheLeague.Shared.Infrastructure.Authorization;

namespace TheLeague.Modules.Subscriptions.Api;

[ApiController]
[Route("api/v1/subscriptions")]
public class SubscriptionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SubscriptionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get current subscription for the authenticated club.
    /// </summary>
    [HttpGet("current")]
    [RequireRole(Roles.ClubManager, Roles.SuperAdmin)]
    public async Task<IActionResult> GetCurrentSubscription()
    {
        var clubId = GetClubId();
        if (clubId is null) return Forbid();

        var query = new GetSubscriptionQuery(clubId.Value);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Message });

        return Ok(result.Data);
    }

    /// <summary>
    /// Upgrade subscription tier.
    /// </summary>
    [HttpPost("upgrade")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> Upgrade([FromBody] UpgradeRequest request)
    {
        var clubId = GetClubId();
        if (clubId is null) return Forbid();

        var command = new UpgradeSubscriptionCommand(clubId.Value, request.NewTier);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Message });

        return Ok(new { subscription = result.Data, message = result.Message });
    }

    /// <summary>
    /// Schedule a subscription downgrade at end of billing period.
    /// </summary>
    [HttpPost("downgrade")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> Downgrade([FromBody] DowngradeRequest request)
    {
        var clubId = GetClubId();
        if (clubId is null) return Forbid();

        var command = new DowngradeSubscriptionCommand(clubId.Value, request.NewTier);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Message });

        return Ok(new { subscription = result.Data, message = result.Message });
    }

    /// <summary>
    /// Get usage statistics for the authenticated club.
    /// </summary>
    [HttpGet("usage")]
    [RequireRole(Roles.ClubManager, Roles.SuperAdmin)]
    public async Task<IActionResult> GetUsage()
    {
        var clubId = GetClubId();
        if (clubId is null) return Forbid();

        var query = new GetUsageQuery(clubId.Value);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Message });

        return Ok(result.Data);
    }

    /// <summary>
    /// List available subscription tiers (public endpoint).
    /// </summary>
    [HttpGet("tiers")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTiers()
    {
        var query = new GetTiersQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    private Guid? GetClubId()
    {
        var clubIdClaim = User.FindFirstValue("clubId");
        if (Guid.TryParse(clubIdClaim, out var clubId))
            return clubId;
        return null;
    }
}

// Request DTOs
public record UpgradeRequest(SubscriptionTier NewTier);
public record DowngradeRequest(SubscriptionTier NewTier);
