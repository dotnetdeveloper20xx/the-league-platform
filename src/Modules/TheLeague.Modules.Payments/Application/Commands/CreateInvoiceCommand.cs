using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Payments.Application.Dtos;
using TheLeague.Modules.Payments.Domain;
using TheLeague.Modules.Payments.Infrastructure.Persistence;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Enums;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Payments.Application.Commands;

public record CreateInvoiceLineItemRequest(
    string Description,
    int Quantity,
    decimal UnitPrice,
    FeeType FeeType);

public record CreateInvoiceCommand(
    Guid MemberId,
    DateTime DueDate,
    string? Notes,
    List<CreateInvoiceLineItemRequest> LineItems
) : IRequest<Result<InvoiceDto>>;

public class CreateInvoiceCommandHandler : IRequestHandler<CreateInvoiceCommand, Result<InvoiceDto>>
{
    private readonly PaymentsDbContext _db;
    private readonly ITenantService _tenantService;

    public CreateInvoiceCommandHandler(PaymentsDbContext db, ITenantService tenantService)
    {
        _db = db;
        _tenantService = tenantService;
    }

    public async Task<Result<InvoiceDto>> Handle(CreateInvoiceCommand request, CancellationToken ct)
    {
        var clubId = _tenantService.CurrentTenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        if (request.LineItems == null || request.LineItems.Count == 0)
            return Result.Failure<InvoiceDto>("Invoice must have at least one line item.");

        if (request.LineItems.Count > 50)
            return Result.Failure<InvoiceDto>("Invoice cannot have more than 50 line items.");

        // Generate next invoice number for this club
        var lastInvoice = await _db.Invoices
            .IgnoreQueryFilters()
            .Where(i => i.ClubId == clubId)
            .OrderByDescending(i => i.CreatedAt)
            .FirstOrDefaultAsync(ct);

        var nextNumber = 1;
        if (lastInvoice != null && lastInvoice.InvoiceNumber.StartsWith("INV-"))
        {
            if (int.TryParse(lastInvoice.InvoiceNumber[4..], out var lastNum))
                nextNumber = lastNum + 1;
        }

        var invoiceNumber = $"INV-{nextNumber:D5}";

        var invoice = Invoice.Create(clubId, request.MemberId, invoiceNumber, request.DueDate, request.Notes);

        foreach (var item in request.LineItems)
        {
            invoice.AddLineItem(item.Description, item.Quantity, item.UnitPrice, item.FeeType);
        }

        _db.Invoices.Add(invoice);
        await _db.SaveChangesAsync(ct);

        var dto = MapToDto(invoice);
        return Result.Success(dto);
    }

    private static InvoiceDto MapToDto(Invoice invoice)
    {
        return new InvoiceDto(
            invoice.Id, invoice.MemberId, invoice.InvoiceNumber, invoice.Status,
            invoice.IssueDate, invoice.DueDate, invoice.TotalAmount, invoice.PaidAmount,
            invoice.Notes,
            invoice.LineItems.Select(li => new InvoiceLineItemDto(
                li.Id, li.Description, li.Quantity, li.UnitPrice, li.TotalPrice, li.FeeType
            )).ToList());
    }
}
