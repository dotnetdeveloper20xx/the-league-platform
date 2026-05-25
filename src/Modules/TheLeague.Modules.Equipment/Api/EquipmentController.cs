using MediatR;
using Microsoft.AspNetCore.Mvc;
using TheLeague.Modules.Equipment.Application.Commands;
using TheLeague.Modules.Equipment.Application.Queries;
using TheLeague.Shared.Domain.Enums;
using TheLeague.Shared.Infrastructure.Authorization;

namespace TheLeague.Modules.Equipment.Api;

[ApiController]
[Route("api/v1/equipment")]
public class EquipmentController : ControllerBase
{
    private readonly IMediator _mediator;

    public EquipmentController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> GetAll(
        [FromQuery] EquipmentCategory? category = null,
        [FromQuery] EquipmentCondition? condition = null,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetEquipmentQuery(category, condition, search, page, pageSize), ct);
        return Ok(result);
    }

    [HttpPost]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> Create([FromBody] CreateEquipmentCommand command, CancellationToken ct)
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
        var result = await _mediator.Send(new GetEquipmentByIdQuery(id), ct);
        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEquipmentRequest request, CancellationToken ct)
    {
        var command = new UpdateEquipmentCommand(
            id, request.Name, request.Category, request.Condition,
            request.Location, request.PurchaseDate, request.Value,
            request.AnnualDepreciationRate, request.SerialNumber, request.IsActive);

        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPost("{id:guid}/loans")]
    [RequireRole(Roles.ClubManager, Roles.Member)]
    public async Task<IActionResult> RequestLoan(Guid id, [FromBody] RequestLoanRequest request, CancellationToken ct)
    {
        var command = new RequestLoanCommand(
            id, request.MemberId, request.LoanDate,
            request.ExpectedReturnDate, request.Fee, request.Deposit, request.Notes);

        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetById), new { id }, result);
    }

    [HttpPost("loans/{id:guid}/return")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> ReturnLoan(Guid id, [FromBody] ReturnLoanRequest? request = null, CancellationToken ct = default)
    {
        var command = new ReturnLoanCommand(id, request?.IsDamaged ?? false);
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("{id:guid}/reservations")]
    [RequireRole(Roles.ClubManager, Roles.Member)]
    public async Task<IActionResult> CreateReservation(Guid id, [FromBody] CreateReservationRequest request, CancellationToken ct)
    {
        var command = new CreateReservationCommand(
            id, request.MemberId, request.StartDate, request.EndDate, request.Notes);

        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetById), new { id }, result);
    }

    [HttpPost("{id:guid}/maintenance")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> RecordMaintenance(Guid id, [FromBody] RecordMaintenanceRequest request, CancellationToken ct)
    {
        var command = new RecordMaintenanceCommand(
            id, request.MaintenanceDate, request.Description,
            request.ResultingCondition, request.Cost, request.PerformedBy);

        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("loans")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> GetLoans(
        [FromQuery] LoanStatus? status = null,
        [FromQuery] bool? overdueOnly = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetLoansQuery(status, overdueOnly, page, pageSize), ct);
        return Ok(result);
    }
}

public record UpdateEquipmentRequest(
    string Name,
    EquipmentCategory Category,
    EquipmentCondition Condition,
    string Location,
    DateTime? PurchaseDate,
    decimal Value,
    decimal AnnualDepreciationRate,
    string? SerialNumber,
    bool IsActive);

public record RequestLoanRequest(
    Guid MemberId,
    DateTime LoanDate,
    DateTime ExpectedReturnDate,
    decimal Fee,
    decimal Deposit,
    string? Notes);

public record ReturnLoanRequest(
    bool IsDamaged = false);

public record CreateReservationRequest(
    Guid MemberId,
    DateTime StartDate,
    DateTime EndDate,
    string? Notes);

public record RecordMaintenanceRequest(
    DateTime MaintenanceDate,
    string Description,
    EquipmentCondition ResultingCondition,
    decimal? Cost,
    string? PerformedBy);
