using MediatR;
using Microsoft.AspNetCore.Mvc;
using TheLeague.Modules.Programs.Application.Commands;
using TheLeague.Modules.Programs.Application.Queries;
using TheLeague.Shared.Infrastructure.Authorization;

namespace TheLeague.Modules.Programs.Api;

[ApiController]
[Route("api/v1/programs")]
public class ProgramsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProgramsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [RequireRole(Roles.ClubManager, Roles.Member)]
    public async Task<IActionResult> GetPrograms(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetProgramsQuery(page, pageSize), ct);
        return Ok(result);
    }

    [HttpPost]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> Create([FromBody] CreateProgramCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [HttpGet("{id:guid}")]
    [RequireRole(Roles.ClubManager, Roles.Member)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetProgramByIdQuery(id), ct);
        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProgramRequest request, CancellationToken ct)
    {
        var command = new UpdateProgramCommand(
            id, request.Name, request.Description,
            request.ProgramType, request.SkillLevel,
            request.Capacity, request.Price,
            request.StartDate, request.EndDate,
            request.IsActive);

        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("{id:guid}/sessions")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> CreateSession(Guid id, [FromBody] CreateProgramSessionRequest request, CancellationToken ct)
    {
        var command = new CreateProgramSessionCommand(
            id, request.Title, request.InstructorId, request.InstructorName,
            request.StartDateTime, request.EndDateTime,
            request.VenueId, request.VenueName,
            request.MaxCapacity, request.SessionOrder);

        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetById), new { id }, result);
    }

    [HttpPost("{id:guid}/enroll")]
    [RequireRole(Roles.ClubManager, Roles.Member)]
    public async Task<IActionResult> Enroll(Guid id, [FromBody] EnrollMemberRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new EnrollMemberCommand(id, request.MemberId), ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("{id:guid}/sessions/{sessionId:guid}/attendance")]
    [RequireRole(Roles.ClubManager, Roles.Coach)]
    public async Task<IActionResult> MarkAttendance(Guid id, Guid sessionId, [FromBody] MarkAttendanceRequest request, CancellationToken ct)
    {
        var command = new MarkProgramAttendanceCommand(id, sessionId, request.Entries);
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("{id:guid}/certificates")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> IssueCertificate(Guid id, [FromBody] IssueCertificateRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new IssueCertificateCommand(id, request.MemberId), ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetMemberCertificates), new { memberId = request.MemberId }, result);
    }

    [HttpDelete("{id:guid}/enrollments/{memberId:guid}")]
    [RequireRole(Roles.ClubManager, Roles.Member)]
    public async Task<IActionResult> WithdrawEnrollment(Guid id, Guid memberId, CancellationToken ct)
    {
        var result = await _mediator.Send(new WithdrawEnrollmentCommand(id, memberId), ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("{id:guid}/enrollments")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> GetEnrollments(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetProgramEnrollmentsQuery(id, page, pageSize), ct);
        return Ok(result);
    }

    [HttpGet("~/api/v1/members/{memberId:guid}/certificates")]
    [RequireRole(Roles.ClubManager, Roles.Member)]
    public async Task<IActionResult> GetMemberCertificates(
        Guid memberId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetMemberCertificatesQuery(memberId, page, pageSize), ct);
        return Ok(result);
    }
}

public record UpdateProgramRequest(
    string Name,
    string? Description,
    TheLeague.Shared.Domain.Enums.ProgramType ProgramType,
    TheLeague.Shared.Domain.Enums.SkillLevel SkillLevel,
    int Capacity,
    decimal Price,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive);

public record CreateProgramSessionRequest(
    string Title,
    Guid? InstructorId,
    string? InstructorName,
    DateTime StartDateTime,
    DateTime EndDateTime,
    Guid? VenueId,
    string? VenueName,
    int MaxCapacity,
    int SessionOrder);

public record EnrollMemberRequest(Guid MemberId);

public record MarkAttendanceRequest(List<AttendanceEntry> Entries);

public record IssueCertificateRequest(Guid MemberId);
