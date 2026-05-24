using TheLeague.Shared.Domain.Entities;
using TheLeague.Shared.Domain.Enums;

namespace TheLeague.Modules.Payments.Domain;

public class Payment : TenantEntity
{
    public Guid MemberId { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentMethod Method { get; private set; }
    public PaymentStatus Status { get; private set; }
    public PaymentType Type { get; private set; }
    public string? ExternalTransactionId { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTime PaymentDate { get; private set; }
    public decimal PlatformFee { get; private set; }
    public string? Description { get; private set; }

    private Payment() { }

    public static Payment Create(
        Guid clubId,
        Guid memberId,
        decimal amount,
        PaymentMethod method,
        PaymentType type,
        string? description = null)
    {
        return new Payment
        {
            ClubId = clubId,
            MemberId = memberId,
            Amount = amount,
            Method = method,
            Type = type,
            Status = PaymentStatus.Pending,
            PaymentDate = DateTime.UtcNow,
            Description = description
        };
    }

    public void Complete(string? externalTransactionId = null, decimal platformFee = 0)
    {
        Status = PaymentStatus.Completed;
        ExternalTransactionId = externalTransactionId;
        PlatformFee = platformFee;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Fail(string reason)
    {
        Status = PaymentStatus.Failed;
        FailureReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Refund()
    {
        Status = PaymentStatus.Refunded;
        UpdatedAt = DateTime.UtcNow;
    }
}
