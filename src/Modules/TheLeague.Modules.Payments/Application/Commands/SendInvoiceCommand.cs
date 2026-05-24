using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Payments.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Payments.Application.Commands;

public record SendInvoiceCommand(Guid InvoiceId) : IRequest<Result>;

public class SendInvoiceCommandHandler : IRequestHandler<SendInvoiceCommand, Result>
{
    private readonly PaymentsDbContext _db;

    public SendInvoiceCommandHandler(PaymentsDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(SendInvoiceCommand request, CancellationToken ct)
    {
        var invoice = await _db.Invoices
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId, ct);

        if (invoice == null)
            return Result.Failure("Invoice not found.");

        try
        {
            invoice.Send();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }

        await _db.SaveChangesAsync(ct);
        return Result.Success("Invoice sent successfully.");
    }
}
