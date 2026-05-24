using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Payments.Application.Dtos;
using TheLeague.Modules.Payments.Domain;
using TheLeague.Modules.Payments.Infrastructure.Persistence;
using TheLeague.Modules.Payments.Infrastructure.Providers;
using TheLeague.Shared.Contracts.Events;
using TheLeague.Shared.Contracts.Messaging;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Enums;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Payments.Application.Commands;

public record ProcessRefundCommand(
    Guid PaymentId,
    decimal Amount,
    string? Reason = null
) : IRequest<Result<RefundDto>>;

public class ProcessRefundCommandHandler : IRequestHandler<ProcessRefundCommand, Result<RefundDto>>
{
    private readonly PaymentsDbContext _db;
    private readonly PaymentProviderFactory _providerFactory;
    private readonly IIntegrationEventBus _eventBus;
    private readonly ITenantService _tenantService;

    public ProcessRefundCommandHandler(
        PaymentsDbContext db,
        PaymentProviderFactory providerFactory,
        IIntegrationEventBus eventBus,
        ITenantService tenantService)
    {
        _db = db;
        _providerFactory = providerFactory;
        _eventBus = eventBus;
        _tenantService = tenantService;
    }

    public async Task<Result<RefundDto>> Handle(ProcessRefundCommand request, CancellationToken ct)
    {
        var clubId = _tenantService.CurrentTenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var payment = await _db.Payments
            .FirstOrDefaultAsync(p => p.Id == request.PaymentId, ct);

        if (payment == null)
            return Result.Failure<RefundDto>("Payment not found.");

        if (payment.Status != PaymentStatus.Completed)
            return Result.Failure<RefundDto>("Can only refund completed payments.");

        if (request.Amount > payment.Amount)
            return Result.Failure<RefundDto>("Refund amount cannot exceed payment amount.");

        var refund = Refund.Create(clubId, payment.Id, payment.MemberId, request.Amount, request.Reason);
        _db.Refunds.Add(refund);

        // Process refund via provider
        if (!string.IsNullOrEmpty(payment.ExternalTransactionId))
        {
            var provider = _providerFactory.GetProvider(payment.Method);
            var result = await provider.ProcessRefundAsync(
                new ProcessRefundRequest(payment.ExternalTransactionId, request.Amount, request.Reason), ct);

            if (!result.Success)
            {
                refund.Fail();
                await _db.SaveChangesAsync(ct);
                return Result.Failure<RefundDto>(result.ErrorMessage ?? "Refund processing failed.");
            }
        }

        refund.Complete();

        // Update member balance
        var balance = await _db.MemberBalances
            .FirstOrDefaultAsync(b => b.MemberId == payment.MemberId, ct);

        if (balance == null)
        {
            balance = MemberBalance.Create(clubId, payment.MemberId);
            _db.MemberBalances.Add(balance);
        }

        balance.DeductCredit(request.Amount);
        balance.AddDebit(request.Amount);

        // Record balance transaction
        var transaction = BalanceTransaction.Create(
            clubId, payment.MemberId, BalanceTransactionType.Debit,
            request.Amount, $"Refund for payment {payment.Id}",
            refund.Id, "Refund");
        _db.BalanceTransactions.Add(transaction);

        // Mark payment as refunded if full refund
        if (request.Amount == payment.Amount)
            payment.Refund();

        await _db.SaveChangesAsync(ct);

        await _eventBus.PublishAsync(
            new RefundProcessedEvent(refund.Id, payment.Id, payment.MemberId, clubId, request.Amount), ct);

        var dto = new RefundDto(
            refund.Id, refund.PaymentId, refund.MemberId,
            refund.Amount, refund.Status, refund.Reason, refund.ProcessedAt);

        return Result.Success(dto);
    }
}
