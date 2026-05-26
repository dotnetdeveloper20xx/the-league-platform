using MediatR;
using Microsoft.AspNetCore.Mvc;
using TheLeague.Modules.Events.Application.Commands;
using TheLeague.Modules.Events.Application.Queries;
using TheLeague.Shared.Domain.Enums;
using TheLeague.Shared.Infrastructure.Authorization;

namespace TheLeague.Modules.Events.Api;

[ApiController]
[Route("api/v1/events")]
public class EventsController : ControllerBase
{
    private readonly IMediator _mediator;

    public EventsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [RequireRole(Roles.ClubManager, Roles.Member)]
    public async Task<IActionResult> GetAll(
        [FromQuery] EventType? eventType = null,
        [FromQuery] EventStatus? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetEventsQuery(eventType, status, page, pageSize), ct);
        return Ok(result);
    }

    [HttpPost]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> Create([FromBody] CreateEventCommand command, CancellationToken ct)
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
        var result = await _mediator.Send(new GetEventByIdQuery(id), ct);
        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEventRequest request, CancellationToken ct)
    {
        var command = new UpdateEventCommand(
            id, request.Title, request.Description, request.EventType,
            request.StartDateTime, request.EndDateTime, request.VenueId,
            request.VenueName, request.Capacity, request.IsTicketed,
            request.StandardPrice, request.MemberPrice, request.AllowRsvp,
            request.CancellationDeadlineHours);

        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("{id:guid}/publish")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> Publish(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new PublishEventCommand(id), ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("{id:guid}/open-registration")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> OpenRegistration(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new OpenRegistrationCommand(id), ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("{id:guid}/register")]
    [RequireRole(Roles.Member, Roles.ClubManager)]
    public async Task<IActionResult> Register(Guid id, [FromBody] EventRegistrationRequest request, CancellationToken ct)
    {
        var command = new RegisterForEventCommand(
            id, request.MemberId, request.IsTicketPurchase,
            request.RsvpResponse, request.GuestCount);

        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("{id:guid}/registrations/{regId:guid}")]
    [RequireRole(Roles.Member, Roles.ClubManager)]
    public async Task<IActionResult> CancelRegistration(Guid id, Guid regId, CancellationToken ct)
    {
        var result = await _mediator.Send(new CancelRegistrationCommand(id, regId), ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("{id:guid}/cancel")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> CancelEvent(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new CancelEventCommand(id), ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("{id:guid}/check-in")]
    [RequireRole(Roles.ClubManager, Roles.Coach)]
    public async Task<IActionResult> CheckIn(Guid id, [FromBody] CheckInRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CheckInCommand(id, request.TicketNumber), ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("member/{memberId:guid}")]
    [RequireRole(Roles.Member, Roles.ClubManager)]
    public async Task<IActionResult> GetMemberEvents(Guid memberId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetMemberEventsQuery(memberId, page, pageSize), ct);
        return Ok(result);
    }
}

public record UpdateEventRequest(
    string Title,
    string? Description,
    EventType EventType,
    DateTime StartDateTime,
    DateTime EndDateTime,
    Guid? VenueId,
    string? VenueName,
    int? Capacity,
    bool IsTicketed,
    decimal? StandardPrice,
    decimal? MemberPrice,
    bool AllowRsvp,
    int CancellationDeadlineHours = 48);

public record EventRegistrationRequest(
    Guid MemberId,
    bool IsTicketPurchase,
    RSVPResponse? RsvpResponse = null,
    int GuestCount = 0);

public record CheckInRequest(string TicketNumber);
