using MediatR;
using Microsoft.AspNetCore.Mvc;
using TheLeague.Modules.Facilities.Application.Commands;
using TheLeague.Modules.Facilities.Application.Queries;
using TheLeague.Shared.Infrastructure.Authorization;

namespace TheLeague.Modules.Facilities.Api;

[ApiController]
[Route("api/v1/facilities")]
public class FacilitiesController : ControllerBase
{
    private readonly IMediator _mediator;

    public FacilitiesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [RequireRole(Roles.ClubManager, Roles.Member)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetFacilitiesQuery(page, pageSize), ct);
        return Ok(result);
    }

    [HttpPost]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> Create([FromBody] CreateFacilityCommand command, CancellationToken ct)
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
        var result = await _mediator.Send(new GetFacilityByIdQuery(id), ct);
        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFacilityRequest request, CancellationToken ct)
    {
        var command = new UpdateFacilityCommand(id, request.Name, request.FacilityType, request.Description, request.Capacity, request.IsActive);
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }

    [HttpGet("{id:guid}/availability")]
    [RequireRole(Roles.ClubManager, Roles.Member)]
    public async Task<IActionResult> GetAvailability(Guid id, [FromQuery] DateOnly date, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetFacilityAvailabilityQuery(id, date), ct);
        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPost("{id:guid}/book")]
    [RequireRole(Roles.ClubManager, Roles.Member)]
    public async Task<IActionResult> Book(Guid id, [FromBody] BookFacilityRequest request, CancellationToken ct)
    {
        var command = new BookFacilityCommand(id, request.MemberId, request.BookingDate, request.StartTime, request.Duration, request.IsMember);
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("bookings/{id:guid}")]
    [RequireRole(Roles.ClubManager, Roles.Member)]
    public async Task<IActionResult> CancelBooking(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new CancelFacilityBookingCommand(id), ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("{id:guid}/maintenance")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> CreateMaintenance(Guid id, [FromBody] CreateMaintenanceRequest request, CancellationToken ct)
    {
        var command = new CreateMaintenanceCommand(id, request.Title, request.Description, request.StartDate, request.EndDate);
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("{id:guid}/blockout")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> CreateBlockout(Guid id, [FromBody] CreateBlockoutRequest request, CancellationToken ct)
    {
        var command = new CreateBlockoutCommand(id, request.Reason, request.StartDate, request.EndDate);
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }
}

public record UpdateFacilityRequest(
    string Name,
    Domain.FacilityType FacilityType,
    string? Description,
    int? Capacity,
    bool IsActive);

public record BookFacilityRequest(
    Guid MemberId,
    DateOnly BookingDate,
    TimeOnly StartTime,
    int Duration,
    bool IsMember);

public record CreateMaintenanceRequest(
    string Title,
    string? Description,
    DateTime StartDate,
    DateTime EndDate);

public record CreateBlockoutRequest(
    string Reason,
    DateTime StartDate,
    DateTime EndDate);
