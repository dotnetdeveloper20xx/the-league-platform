using TheLeague.Shared.Domain.Entities;

namespace TheLeague.Modules.Payments.Domain;

public class JournalEntryLine : BaseEntity
{
    public Guid JournalEntryId { get; private set; }
    public Guid AccountId { get; private set; }
    public decimal DebitAmount { get; private set; }
    public decimal CreditAmount { get; private set; }
    public string? Description { get; private set; }

    private JournalEntryLine() { }

    public static JournalEntryLine Create(
        Guid journalEntryId,
        Guid accountId,
        decimal debitAmount,
        decimal creditAmount,
        string? description = null)
    {
        return new JournalEntryLine
        {
            JournalEntryId = journalEntryId,
            AccountId = accountId,
            DebitAmount = debitAmount,
            CreditAmount = creditAmount,
            Description = description
        };
    }
}
