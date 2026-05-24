using TheLeague.Shared.Domain.Entities;

namespace TheLeague.Modules.Payments.Domain;

public class JournalEntry : TenantEntity
{
    public DateTime EntryDate { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string? ReferenceNumber { get; private set; }
    public bool IsPosted { get; private set; }

    public List<JournalEntryLine> Lines { get; private set; } = new();

    private JournalEntry() { }

    public static JournalEntry Create(
        Guid clubId,
        DateTime entryDate,
        string description,
        string? referenceNumber = null)
    {
        return new JournalEntry
        {
            ClubId = clubId,
            EntryDate = entryDate,
            Description = description,
            ReferenceNumber = referenceNumber,
            IsPosted = false
        };
    }

    public void AddLine(Guid accountId, decimal debitAmount, decimal creditAmount, string? description = null)
    {
        var line = JournalEntryLine.Create(Id, accountId, debitAmount, creditAmount, description);
        Lines.Add(line);
    }

    public bool IsBalanced()
    {
        var totalDebits = Lines.Sum(l => l.DebitAmount);
        var totalCredits = Lines.Sum(l => l.CreditAmount);
        return totalDebits == totalCredits;
    }

    public void Post()
    {
        if (!IsBalanced())
            throw new InvalidOperationException("Cannot post an unbalanced journal entry. Total debits must equal total credits.");

        IsPosted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
