using TheLeague.Shared.Domain.Entities;

namespace TheLeague.Modules.Payments.Domain;

public enum BalanceTransactionType
{
    Credit,
    Debit
}

public class BalanceTransaction : TenantEntity
{
    public Guid MemberId { get; private set; }
    public BalanceTransactionType Type { get; private set; }
    public decimal Amount { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public Guid? ReferenceId { get; private set; }
    public string? ReferenceType { get; private set; }
    public DateTime TransactionDate { get; private set; }

    private BalanceTransaction() { }

    public static BalanceTransaction Create(
        Guid clubId,
        Guid memberId,
        BalanceTransactionType type,
        decimal amount,
        string description,
        Guid? referenceId = null,
        string? referenceType = null)
    {
        return new BalanceTransaction
        {
            ClubId = clubId,
            MemberId = memberId,
            Type = type,
            Amount = amount,
            Description = description,
            ReferenceId = referenceId,
            ReferenceType = referenceType,
            TransactionDate = DateTime.UtcNow
        };
    }
}
