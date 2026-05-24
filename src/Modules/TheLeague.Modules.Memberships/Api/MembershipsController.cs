using MediatR;
using Microsoft.AspNetCore.Mvc;
using TheLeague.Modules.Memberships.Application.Commands;
using TheLeague.Modules.Memberships.Application.Queries;
using TheLeague.Shared.Infrastructure.Authorization;

namespace TheLeague.Modules.Memberships.Api;

[ApiController]
[Route("api/v1")]
public class MembershipsController : ControllerBase
{
    private readonly IMediator _mediator;

    public MembershipsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // Membership Types

    [HttpGet("membership-types")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> GetMembershipTypes([FromQuery] Guid clubId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetMembershipTypesQuery(clubId), ct);
        return Ok(result);
    }

    [HttpPost("membership-types")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> CreateMembershipType([FromBody] CreateMembershipTypeCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetMembershipTypes), new { clubId = command.ClubId }, result);
    }

    [HttpPut("membership-types/{id:guid}")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> UpdateMembershipType(Guid id, [FromBody] UpdateMembershipTypeRequest request, CancellationToken ct)
    {
        var command = new UpdateMembershipTypeCommand(
            id, request.Name, request.Description, request.Price, request.BillingCycle,
            request.MinAge, request.MaxAge, request.Capacity, request.JoiningFee,
            request.IsActive, request.AllowAutoRenewal, request.FreezeFee);

        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }

    // Memberships

    [HttpPost("memberships/enroll")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> EnrollMember([FromBody] EnrollMemberCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetMemberMemberships), new { memberId = command.MemberId }, result);
    }

    [HttpPost("memberships/{id:guid}/renew")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> RenewMembership(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new RenewMembershipCommand(id), ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("memberships/{id:guid}/freeze")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> FreezeMembership(Guid id, [FromBody] FreezeMembershipRequest request, CancellationToken ct)
    {
        var command = new FreezeMembershipCommand(id, request.DurationDays, request.Reason);
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("memberships/{id:guid}/cancel")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> CancelMembership(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new CancelMembershipCommand(id), ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("memberships/member/{memberId:guid}")]
    [RequireRole(Roles.ClubManager, Roles.Member)]
    public async Task<IActionResult> GetMemberMemberships(Guid memberId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetMemberMembershipsQuery(memberId), ct);
        return Ok(result);
    }

    // Waitlist

    [HttpGet("membership-types/{id:guid}/waitlist")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> GetWaitlist(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetWaitlistQuery(id), ct);
        return Ok(result);
    }

    [HttpPost("membership-types/{id:guid}/waitlist")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> JoinWaitlist(Guid id, [FromBody] JoinWaitlistRequest request, CancellationToken ct)
    {
        var command = new JoinWaitlistCommand(request.ClubId, id, request.MemberId);
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetWaitlist), new { id }, result);
    }

    [HttpPost("membership-types/{id:guid}/waitlist/promote")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> PromoteFromWaitlist(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new PromoteFromWaitlistCommand(id), ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    // Discounts

    [HttpGet("membership-discounts")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> GetDiscounts([FromQuery] Guid clubId, [FromQuery] Guid? membershipTypeId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetDiscountsQuery(clubId, membershipTypeId), ct);
        return Ok(result);
    }

    [HttpPost("membership-discounts")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> ApplyDiscount([FromBody] ApplyDiscountCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetDiscounts), new { clubId = command.ClubId }, result);
    }
}

// Request DTOs for endpoints that need body parameters separate from the route
public record UpdateMembershipTypeRequest(
    string Name,
    string? Description,
    decimal Price,
    TheLeague.Shared.Domain.Enums.BillingCycle BillingCycle,
    int? MinAge,
    int? MaxAge,
    int? Capacity,
    decimal? JoiningFee,
    bool IsActive,
    bool AllowAutoRenewal,
    decimal? FreezeFee
);

public record FreezeMembershipRequest(int DurationDays, string? Reason);

public record JoinWaitlistRequest(Guid ClubId, Guid MemberId);
