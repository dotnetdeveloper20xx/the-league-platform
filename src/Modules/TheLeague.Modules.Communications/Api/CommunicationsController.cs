using MediatR;
using Microsoft.AspNetCore.Mvc;
using TheLeague.Modules.Communications.Application.Commands;
using TheLeague.Modules.Communications.Application.Queries;
using TheLeague.Shared.Infrastructure.Authorization;

namespace TheLeague.Modules.Communications.Api;

[ApiController]
[Route("api/v1/communications")]
public class CommunicationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CommunicationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("templates")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> GetTemplates([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetTemplatesQuery(page, pageSize), ct);
        return Ok(result);
    }

    [HttpPost("templates")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> CreateTemplate([FromBody] CreateTemplateCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetTemplates), result);
    }

    [HttpPut("templates/{id:guid}")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> UpdateTemplate(Guid id, [FromBody] UpdateTemplateRequest request, CancellationToken ct)
    {
        var command = new UpdateTemplateCommand(id, request.Name, request.TemplateType, request.Subject, request.Body, request.IsActive);
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPost("campaigns")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> SendBulkCampaign([FromBody] SendBulkCampaignCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("campaigns")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> GetCampaigns([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetCampaignsQuery(page, pageSize), ct);
        return Ok(result);
    }

    [HttpGet("email-logs")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> GetEmailLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetEmailLogsQuery(page, pageSize), ct);
        return Ok(result);
    }

    [HttpPost("sms")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> SendSms([FromBody] SendSmsCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }
}

public record UpdateTemplateRequest(
    string Name,
    string TemplateType,
    string Subject,
    string Body,
    bool IsActive
);
