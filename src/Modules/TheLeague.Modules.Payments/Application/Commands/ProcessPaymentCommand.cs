using MediatR;
using TheLeague.Modules.Payments.Application.Dtos;
using TheLeague.Modules.Payments.Domain;
using TheLeague.Modules.Payments.Infrastructure;
using TheLeague.Modules.Payments.Infrastructure.Persistence;
using TheLeague.Modules.Payments.Infrastructure.Providers;
using TheLeague.Shared.Contracts.Events;
using TheLeague.Shared.Contracts.Messaging;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Enums;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Payments.Application.Commands;

public record ProcessPaymentCommand(
    Guid MemberId,
    decimal Amount,
    PaymentMethod Method,
    PaymentType Type,
    string? Description = null
) : IRequest<Result<PaymentDto>>;

public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, Result<PaymentDto>>
{
    private readonly PaymentsDbContext _db;
    private readonly PaymentProviderFactory _providerFactory;
    private readonly PlatformFeeCalculator _feeCalculator;
    private readonly IIntegrationEventBus _eventBus;
    private readonly ITenantService _tenantService;

    public ProcessPaymentCommandHandler(
        PaymentsDbContext db,
        PaymentProviderFactory providerFactory,
        PlatformFeeCalculator feeCalculator,
        IIntegrationEventBus eventBus,
        ITenantService tenantService)
    {
        _db = db;
        _providerFactory = providerFactory;
        _feeCalculator = feeCalculator;
        _eventBus = eventBus;
        _tenantService = tenantService;
    }

    public async Task<Result<PaymentDto>> Handle(ProcessPaymentCommand request, CancellationToken ct)
    {
        var clubId = _tenantService.CurrentTenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var payment = Payment.Create(clubId, request.MemberId, request.Amount, request.Method, request.Type, request.Description);
        _db.Payments.Add(payment);

        var provider = _providerFactory.GetProvider(request.Method);
        var result = await provider.ProcessPaymentAsync(
            new ProcessPaymentRequest(request.Amount, "GBP", request.MemberId, request.Description, null), ct);

        if (result.Success)
        {
            var platformFee = _feeCalculator.CalculateFee(request.Amount);
            payment.Complete(result.TransactionId, platformFee);

            await _db.SaveChangesAsync(ct);
            await _eventBus.PublishAsync(
                new PaymentCompletedEvent(payment.Id, request.MemberId, clubId, request.Amount, request.Type), ct);
        }
        else
        {
            payment.Fail(result.ErrorMessage ?? "Unknown payment failure");
            await _db.SaveChangesAsync(ct);
            await _eventBus.PublishAsync(
                new PaymentFailedEvent(payment.Id, request.MemberId, clubId, result.ErrorMessage ?? "Unknown"), ct);
        }

        var dto = new PaymentDto(
            payment.Id, payment.MemberId, payment.Amount, payment.Method,
            payment.Status, payment.Type, payment.ExternalTransactionId,
            payment.FailureReason, payment.PaymentDate, payment.PlatformFee, payment.Description);

        return Result.Success(dto);
    }
}
