using MediatR;
using Microsoft.AspNetCore.Mvc;
using TheLeague.Modules.Payments.Application.Commands;
using TheLeague.Modules.Payments.Application.Queries;
using TheLeague.Shared.Domain.Enums;
using TheLeague.Shared.Infrastructure.Authorization;

namespace TheLeague.Modules.Payments.Api;

[ApiController]
[Route("api/v1")]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // POST /api/v1/payments
    [HttpPost("payments")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    // GET /api/v1/payments
    [HttpGet("payments")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> GetPayments(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] PaymentStatus? status = null,
        [FromQuery] PaymentMethod? method = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetPaymentsQuery(page, pageSize, fromDate, toDate, status, method), ct);
        return Ok(result);
    }

    // POST /api/v1/invoices
    [HttpPost("invoices")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetInvoiceById), new { id = result.Data!.Id }, result);
    }

    // GET /api/v1/invoices
    [HttpGet("invoices")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> GetInvoices(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] InvoiceStatus? status = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetInvoicesQuery(page, pageSize, status), ct);
        return Ok(result);
    }

    // GET /api/v1/invoices/{id}
    [HttpGet("invoices/{id:guid}")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> GetInvoiceById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetInvoiceByIdQuery(id), ct);
        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }

    // POST /api/v1/invoices/{id}/send
    [HttpPost("invoices/{id:guid}/send")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> SendInvoice(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new SendInvoiceCommand(id), ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    // POST /api/v1/invoices/{id}/void
    [HttpPost("invoices/{id:guid}/void")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> VoidInvoice(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new VoidInvoiceCommand(id), ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    // POST /api/v1/invoices/{id}/payment
    [HttpPost("invoices/{id:guid}/payment")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> RecordInvoicePayment(Guid id, [FromBody] RecordInvoicePaymentRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new RecordInvoicePaymentCommand(id, request.Amount), ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    // POST /api/v1/refunds
    [HttpPost("refunds")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> ProcessRefund([FromBody] ProcessRefundCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    // GET /api/v1/members/{memberId}/balance
    [HttpGet("members/{memberId:guid}/balance")]
    [RequireRole(Roles.ClubManager, Roles.Member)]
    public async Task<IActionResult> GetMemberBalance(Guid memberId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetMemberBalanceQuery(memberId), ct);
        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }

    // POST /api/v1/payment-plans
    [HttpPost("payment-plans")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> CreatePaymentPlan([FromBody] CreatePaymentPlanCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    // POST /api/v1/journal-entries
    [HttpPost("journal-entries")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> CreateJournalEntry([FromBody] CreateJournalEntryCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }
}

public record RecordInvoicePaymentRequest(decimal Amount);
