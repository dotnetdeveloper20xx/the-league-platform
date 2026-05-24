using TheLeague.Shared.Domain.Entities;
using TheLeague.Shared.Domain.Enums;

namespace TheLeague.Modules.Payments.Domain;

public class Refund : TenantEntity
{
    public Guid PaymentId { get; private set; }
    public Guid MemberId { get; private set; }
    public decimal Amount { get; private set; }
    public RefundStatus Status { get; private set; }
    public string? Reason { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    private Refund() { }

    public static Refund Create(
        Guid clubId,
        Guid paymentId,
        Guid memberId,
        decimal amount,
        string? reason = null)
    {
        return new Refund
        {
            ClubId = clubId,
            PaymentId = paymentId,
            MemberId = memberId,
            Amount = amount,
            Status = RefundStatus.Requested,
            Reason = reason
        };
    }

    public void Approve()
    {
        Status = RefundStatus.Approved;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Process()
    {
        Status = RefundStatus.Processing;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        Status = RefundStatus.Completed;
        ProcessedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Fail()
    {
        Status = RefundStatus.Failed;
        UpdatedAt = DateTime.UtcNow;
    }
}
