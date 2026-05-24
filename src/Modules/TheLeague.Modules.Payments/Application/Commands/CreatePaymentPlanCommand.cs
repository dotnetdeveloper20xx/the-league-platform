using MediatR;
using TheLeague.Modules.Payments.Application.Dtos;
using TheLeague.Modules.Payments.Domain;
using TheLeague.Modules.Payments.Infrastructure.Persistence;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Payments.Application.Commands;

public record CreatePaymentPlanCommand(
    Guid MemberId,
    Guid InvoiceId,
    decimal TotalAmount,
    int InstallmentCount,
    string Frequency,
    DateTime StartDate
) : IRequest<Result<Guid>>;

public class CreatePaymentPlanCommandHandler : IRequestHandler<CreatePaymentPlanCommand, Result<Guid>>
{
    private readonly PaymentsDbContext _db;
    private readonly ITenantService _tenantService;

    public CreatePaymentPlanCommandHandler(PaymentsDbContext db, ITenantService tenantService)
    {
        _db = db;
        _tenantService = tenantService;
    }

    public async Task<Result<Guid>> Handle(CreatePaymentPlanCommand request, CancellationToken ct)
    {
        var clubId = _tenantService.CurrentTenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        if (request.InstallmentCount <= 0)
            return Result.Failure<Guid>("Installment count must be greater than zero.");

        var plan = PaymentPlan.Create(
            clubId, request.MemberId, request.InvoiceId,
            request.TotalAmount, request.InstallmentCount,
            request.Frequency, request.StartDate);

        _db.PaymentPlans.Add(plan);

        // Generate installments
        var installmentAmount = Math.Round(request.TotalAmount / request.InstallmentCount, 2);
        var remainder = request.TotalAmount - (installmentAmount * request.InstallmentCount);

        for (int i = 1; i <= request.InstallmentCount; i++)
        {
            var amount = installmentAmount;
            if (i == request.InstallmentCount)
                amount += remainder; // Add remainder to last installment

            var dueDate = CalculateDueDate(request.StartDate, request.Frequency, i);

            var installment = PaymentInstallment.Create(clubId, plan.Id, i, amount, dueDate);
            plan.Installments.Add(installment);
        }

        await _db.SaveChangesAsync(ct);
        return Result.Success(plan.Id);
    }

    private static DateTime CalculateDueDate(DateTime startDate, string frequency, int installmentNumber)
    {
        return frequency.ToLowerInvariant() switch
        {
            "weekly" => startDate.AddDays(7 * (installmentNumber - 1)),
            "fortnightly" => startDate.AddDays(14 * (installmentNumber - 1)),
            "monthly" => startDate.AddMonths(installmentNumber - 1),
            "quarterly" => startDate.AddMonths(3 * (installmentNumber - 1)),
            _ => startDate.AddMonths(installmentNumber - 1) // Default to monthly
        };
    }
}
