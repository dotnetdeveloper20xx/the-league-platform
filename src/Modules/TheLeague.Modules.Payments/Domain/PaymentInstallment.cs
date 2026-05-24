using TheLeague.Shared.Domain.Entities;

namespace TheLeague.Modules.Payments.Domain;

public enum InstallmentStatus
{
    Pending,
    Processing,
    Completed,
    Failed,
    Refunded,
    Cancelled
}

public class PaymentInstallment : TenantEntity
{
    public Guid PaymentPlanId { get; private set; }
    public int InstallmentNumber { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime DueDate { get; private set; }
    public InstallmentStatus Status { get; private set; }
    public DateTime? PaidDate { get; private set; }

    private PaymentInstallment() { }

    public static PaymentInstallment Create(
        Guid clubId,
        Guid paymentPlanId,
        int installmentNumber,
        decimal amount,
        DateTime dueDate)
    {
        return new PaymentInstallment
        {
            ClubId = clubId,
            PaymentPlanId = paymentPlanId,
            InstallmentNumber = installmentNumber,
            Amount = amount,
            DueDate = dueDate,
            Status = InstallmentStatus.Pending
        };
    }

    public void MarkCompleted()
    {
        Status = InstallmentStatus.Completed;
        PaidDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkFailed()
    {
        Status = InstallmentStatus.Failed;
        UpdatedAt = DateTime.UtcNow;
    }
}
