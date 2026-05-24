using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Payments.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Payments.Application.Commands;

public record VoidInvoiceCommand(Guid InvoiceId) : IRequest<Result>;

public class VoidInvoiceCommandHandler : IRequestHandler<VoidInvoiceCommand, Result>
{
    private readonly PaymentsDbContext _db;

    public VoidInvoiceCommandHandler(PaymentsDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(VoidInvoiceCommand request, CancellationToken ct)
    {
        var invoice = await _db.Invoices
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId, ct);

        if (invoice == null)
            return Result.Failure("Invoice not found.");

        try
        {
            invoice.Void();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }

        await _db.SaveChangesAsync(ct);
        return Result.Success("Invoice voided successfully.");
    }
}
