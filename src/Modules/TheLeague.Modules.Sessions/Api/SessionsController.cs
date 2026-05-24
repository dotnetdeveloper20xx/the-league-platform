using MediatR;
using Microsoft.AspNetCore.Mvc;
using TheLeague.Modules.Sessions.Application.Commands;
using TheLeague.Modules.Sessions.Application.Queries;
using TheLeague.Shared.Infrastructure.Authorization;

namespace TheLeague.Modules.Sessions.Api;

[ApiController]
[Route("api/v1/sessions")]
public class SessionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SessionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [RequireRole(Roles.ClubManager, Roles.Member)]
    public async Task<IActionResult> GetSessions(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetSessionsQuery(fromDate, toDate, page, pageSize), ct);
        return Ok(result);
    }

    [HttpPost]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> Create([FromBody] CreateSessionCommand command, CancellationToken ct)
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
        var result = await _mediator.Send(new GetSessionByIdQuery(id), ct);
        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPost("{id:guid}/book")]
    [RequireRole(Roles.Member, Roles.ClubManager)]
    public async Task<IActionResult> Book(Guid id, [FromBody] BookSessionRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new BookSessionCommand(id, request.MemberId), ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("{id:guid}/bookings/{bookingId:guid}")]
    [RequireRole(Roles.Member, Roles.ClubManager)]
    public async Task<IActionResult> CancelBooking(Guid id, Guid bookingId, CancellationToken ct)
    {
        var result = await _mediator.Send(new CancelBookingCommand(id, bookingId), ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("{id:guid}/cancel")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> CancelSession(Guid id, [FromBody] CancelSessionRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CancelSessionCommand(id, request.Reason), ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("{id:guid}/attendance")]
    [RequireRole(Roles.ClubManager, Roles.Coach)]
    public async Task<IActionResult> MarkAttendance(Guid id, [FromBody] MarkAttendanceCommand command, CancellationToken ct)
    {
        var cmd = new MarkAttendanceCommand(id, command.Entries);
        var result = await _mediator.Send(cmd, ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("~/api/v1/recurring-schedules")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> GetRecurringSchedules(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetRecurringSchedulesQuery(page, pageSize), ct);
        return Ok(result);
    }

    [HttpPost("~/api/v1/recurring-schedules")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> CreateRecurringSchedule([FromBody] CreateRecurringScheduleCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetRecurringSchedules), result);
    }

    [HttpGet("~/api/v1/members/{memberId:guid}/bookings")]
    [RequireRole(Roles.Member, Roles.ClubManager)]
    public async Task<IActionResult> GetMemberBookings(
        Guid memberId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetMemberBookingsQuery(memberId, page, pageSize), ct);
        return Ok(result);
    }
}

public record BookSessionRequest(Guid MemberId);
public record CancelSessionRequest(string Reason);
