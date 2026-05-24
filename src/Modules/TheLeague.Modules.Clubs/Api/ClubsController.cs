using MediatR;
using Microsoft.AspNetCore.Mvc;
using TheLeague.Modules.Clubs.Application.Commands;
using TheLeague.Modules.Clubs.Application.Queries;
using TheLeague.Shared.Infrastructure.Authorization;

namespace TheLeague.Modules.Clubs.Api;

[ApiController]
[Route("api/v1/clubs")]
public class ClubsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ClubsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [RequireRole(Roles.SuperAdmin)]
    public async Task<IActionResult> Create([FromBody] CreateClubCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [HttpGet]
    [RequireRole(Roles.SuperAdmin)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetClubsQuery(page, pageSize), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [RequireRole(Roles.SuperAdmin, Roles.ClubManager)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetClubByIdQuery(id), ct);
        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [RequireRole(Roles.SuperAdmin, Roles.ClubManager)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateClubRequest request, CancellationToken ct)
    {
        var command = new UpdateClubCommand(id, request.Name, request.Description,
            request.ContactEmail, request.ContactPhone, request.Address, request.Website);

        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPut("{id:guid}/branding")]
    [RequireRole(Roles.SuperAdmin, Roles.ClubManager)]
    public async Task<IActionResult> UpdateBranding(Guid id, [FromBody] UpdateClubBrandingRequest request, CancellationToken ct)
    {
        var command = new UpdateClubBrandingCommand(id, request.PrimaryColor,
            request.SecondaryColor, request.AccentColor, request.LogoUrl);

        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPost("{id:guid}/deactivate")]
    [RequireRole(Roles.SuperAdmin)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeactivateClubCommand(id), ct);
        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }

    [HttpGet("{id:guid}/settings")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> GetSettings(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetClubSettingsQuery(id), ct);
        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }
}

public record UpdateClubRequest(
    string Name,
    string? Description,
    string? ContactEmail,
    string? ContactPhone,
    string? Address,
    string? Website
);

public record UpdateClubBrandingRequest(
    string PrimaryColor,
    string SecondaryColor,
    string? AccentColor,
    string? LogoUrl
);
