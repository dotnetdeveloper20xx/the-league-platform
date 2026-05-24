using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Payments.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Payments.Application.Commands;

public record RecordInvoicePaymentCommand(Guid InvoiceId, decimal Amount) : IRequest<Result>;

public class RecordInvoicePaymentCommandHandler : IRequestHandler<RecordInvoicePaymentCommand, Result>
{
    private readonly PaymentsDbContext _db;

    public RecordInvoicePaymentCommandHandler(PaymentsDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(RecordInvoicePaymentCommand request, CancellationToken ct)
    {
        var invoice = await _db.Invoices
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId, ct);

        if (invoice == null)
            return Result.Failure("Invoice not found.");

        if (request.Amount <= 0)
            return Result.Failure("Payment amount must be greater than zero.");

        try
        {
            invoice.RecordPayment(request.Amount);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }

        await _db.SaveChangesAsync(ct);
        return Result.Success($"Payment of {request.Amount:C} recorded against invoice {invoice.InvoiceNumber}.");
    }
}
