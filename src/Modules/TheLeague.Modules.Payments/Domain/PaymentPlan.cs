using TheLeague.Shared.Domain.Entities;

namespace TheLeague.Modules.Payments.Domain;

public enum PaymentPlanStatus
{
    Active,
    Completed,
    Cancelled
}

public class PaymentPlan : TenantEntity
{
    public Guid MemberId { get; private set; }
    public Guid InvoiceId { get; private set; }
    public decimal TotalAmount { get; private set; }
    public int InstallmentCount { get; private set; }
    public string Frequency { get; private set; } = string.Empty;
    public DateTime StartDate { get; private set; }
    public PaymentPlanStatus Status { get; private set; }

    public List<PaymentInstallment> Installments { get; private set; } = new();

    private PaymentPlan() { }

    public static PaymentPlan Create(
        Guid clubId,
        Guid memberId,
        Guid invoiceId,
        decimal totalAmount,
        int installmentCount,
        string frequency,
        DateTime startDate)
    {
        return new PaymentPlan
        {
            ClubId = clubId,
            MemberId = memberId,
            InvoiceId = invoiceId,
            TotalAmount = totalAmount,
            InstallmentCount = installmentCount,
            Frequency = frequency,
            StartDate = startDate,
            Status = PaymentPlanStatus.Active
        };
    }

    public void Complete()
    {
        Status = PaymentPlanStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        Status = PaymentPlanStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }
}
