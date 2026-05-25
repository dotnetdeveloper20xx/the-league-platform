using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TheLeague.Modules.Documents.Application.Commands;
using TheLeague.Modules.Documents.Application.Queries;
using TheLeague.Shared.Infrastructure.Authorization;

namespace TheLeague.Modules.Documents.Api;

[ApiController]
[Route("api/v1/documents")]
public class DocumentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DocumentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("upload")]
    [RequireRole(Roles.ClubManager, Roles.Member)]
    public async Task<IActionResult> Upload(IFormFile file, [FromForm] Guid? memberId, [FromForm] string documentType, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest("No file provided.");

        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        await using var stream = file.OpenReadStream();

        var command = new UploadDocumentCommand(
            MemberId: memberId,
            FileName: file.FileName,
            ContentType: file.ContentType,
            FileSize: file.Length,
            DocumentType: documentType,
            FileStream: stream,
            UploadedByUserId: userId.Value);

        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [HttpGet]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? documentType = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetDocumentsQuery(
            DocumentType: documentType,
            Page: page,
            PageSize: pageSize), ct);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [RequireRole(Roles.ClubManager, Roles.Member)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetDocumentByIdQuery(id), ct);
        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }

    [HttpGet("{id:guid}/download")]
    [RequireRole(Roles.ClubManager, Roles.Member)]
    public async Task<IActionResult> Download(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GenerateDownloadUrlCommand(id), ct);
        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(new { DownloadUrl = result.Data });
    }

    [HttpDelete("{id:guid}")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteDocumentCommand(id), ct);
        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }

    [HttpGet("member/{memberId:guid}")]
    [RequireRole(Roles.ClubManager, Roles.Member)]
    public async Task<IActionResult> GetByMember(Guid memberId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetDocumentsQuery(
            MemberId: memberId,
            Page: page,
            PageSize: pageSize), ct);

        return Ok(result);
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? User.FindFirst("sub")?.Value;

        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
