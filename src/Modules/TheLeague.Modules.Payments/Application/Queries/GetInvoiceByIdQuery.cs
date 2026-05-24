using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Payments.Application.Dtos;
using TheLeague.Modules.Payments.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Payments.Application.Queries;

public record GetInvoiceByIdQuery(Guid InvoiceId) : IRequest<Result<InvoiceDto>>;

public class GetInvoiceByIdQueryHandler : IRequestHandler<GetInvoiceByIdQuery, Result<InvoiceDto>>
{
    private readonly PaymentsDbContext _db;

    public GetInvoiceByIdQueryHandler(PaymentsDbContext db)
    {
        _db = db;
    }

    public async Task<Result<InvoiceDto>> Handle(GetInvoiceByIdQuery request, CancellationToken ct)
    {
        var invoice = await _db.Invoices
            .AsNoTracking()
            .Include(i => i.LineItems)
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId, ct);

        if (invoice == null)
            return Result.Failure<InvoiceDto>("Invoice not found.");

        var dto = new InvoiceDto(
            invoice.Id, invoice.MemberId, invoice.InvoiceNumber, invoice.Status,
            invoice.IssueDate, invoice.DueDate, invoice.TotalAmount, invoice.PaidAmount,
            invoice.Notes,
            invoice.LineItems.Select(li => new InvoiceLineItemDto(
                li.Id, li.Description, li.Quantity, li.UnitPrice, li.TotalPrice, li.FeeType
            )).ToList());

        return Result.Success(dto);
    }
}
