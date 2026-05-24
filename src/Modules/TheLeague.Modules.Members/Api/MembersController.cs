using MediatR;
using Microsoft.AspNetCore.Mvc;
using TheLeague.Modules.Members.Application.Commands;
using TheLeague.Modules.Members.Application.Queries;
using TheLeague.Shared.Domain.Enums;
using TheLeague.Shared.Infrastructure.Authorization;

namespace TheLeague.Modules.Members.Api;

[ApiController]
[Route("api/v1/members")]
public class MembersController : ControllerBase
{
    private readonly IMediator _mediator;

    public MembersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search = null,
        [FromQuery] MemberStatus? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetMembersQuery(search, status, page, pageSize), ct);
        return Ok(result);
    }

    [HttpPost]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> Create([FromBody] CreateMemberCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [HttpGet("{id:guid}")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetMemberByIdQuery(id), ct);
        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMemberRequest request, CancellationToken ct)
    {
        var command = new UpdateMemberCommand(
            id, request.FirstName, request.LastName, request.Email,
            request.Phone, request.DateOfBirth, request.Gender,
            request.Address, request.PrimaryEmergencyContact,
            request.SecondaryEmergencyContact, request.MedicalInfo,
            request.ProfilePhotoUrl, request.FacebookUrl,
            request.TwitterHandle, request.InstagramHandle, request.LinkedInUrl,
            request.CustomFieldValues, request.MarketingOptIn,
            request.SmsOptIn, request.EmailOptIn);

        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPost("{id:guid}/status")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeStatusRequest request, CancellationToken ct)
    {
        var command = new ChangeMemberStatusCommand(id, request.NewStatus, request.ChangedByUserId);
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("{id:guid}/family")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> GetFamilyMembers(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetFamilyMembersQuery(id), ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/family")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> AddFamilyMember(Guid id, [FromBody] AddFamilyMemberRequest request, CancellationToken ct)
    {
        var command = new CreateFamilyMemberCommand(id, request.DependentMemberId, request.Relationship);
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetFamilyMembers), new { id }, result);
    }

    [HttpDelete("{id:guid}/family/{familyId:guid}")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> RemoveFamilyMember(Guid id, Guid familyId, CancellationToken ct)
    {
        var result = await _mediator.Send(new RemoveFamilyMemberCommand(familyId), ct);
        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPost("import")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> Import([FromBody] ImportMembersCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }
}

public record UpdateMemberRequest(
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    DateTime? DateOfBirth,
    Gender? Gender,
    TheLeague.Shared.Domain.ValueObjects.Address? Address,
    TheLeague.Shared.Domain.ValueObjects.EmergencyContact? PrimaryEmergencyContact,
    TheLeague.Shared.Domain.ValueObjects.EmergencyContact? SecondaryEmergencyContact,
    TheLeague.Shared.Domain.ValueObjects.MedicalInfo? MedicalInfo,
    string? ProfilePhotoUrl,
    string? FacebookUrl,
    string? TwitterHandle,
    string? InstagramHandle,
    string? LinkedInUrl,
    string? CustomFieldValues,
    bool MarketingOptIn,
    bool SmsOptIn,
    bool EmailOptIn
);

public record ChangeStatusRequest(
    MemberStatus NewStatus,
    string? ChangedByUserId
);

public record AddFamilyMemberRequest(
    Guid DependentMemberId,
    FamilyMemberRelation Relationship
);
