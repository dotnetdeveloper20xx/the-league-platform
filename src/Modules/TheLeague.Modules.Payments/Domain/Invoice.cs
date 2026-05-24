using TheLeague.Shared.Domain.Entities;
using TheLeague.Shared.Domain.Enums;

namespace TheLeague.Modules.Payments.Domain;

public class Invoice : TenantEntity
{
    public Guid MemberId { get; private set; }
    public string InvoiceNumber { get; private set; } = string.Empty;
    public InvoiceStatus Status { get; private set; }
    public DateTime IssueDate { get; private set; }
    public DateTime DueDate { get; private set; }
    public decimal TotalAmount { get; private set; }
    public decimal PaidAmount { get; private set; }
    public string? Notes { get; private set; }

    public List<InvoiceLineItem> LineItems { get; private set; } = new();

    private Invoice() { }

    public static Invoice Create(
        Guid clubId,
        Guid memberId,
        string invoiceNumber,
        DateTime dueDate,
        string? notes = null)
    {
        return new Invoice
        {
            ClubId = clubId,
            MemberId = memberId,
            InvoiceNumber = invoiceNumber,
            Status = InvoiceStatus.Draft,
            IssueDate = DateTime.UtcNow,
            DueDate = dueDate,
            Notes = notes
        };
    }

    public void AddLineItem(string description, int quantity, decimal unitPrice, FeeType feeType)
    {
        var lineItem = InvoiceLineItem.Create(ClubId, Id, description, quantity, unitPrice, feeType);
        LineItems.Add(lineItem);
        TotalAmount = LineItems.Sum(li => li.TotalPrice);
    }

    public void Send()
    {
        if (Status != InvoiceStatus.Draft)
            throw new InvalidOperationException($"Cannot send invoice in status {Status}. Must be Draft.");

        Status = InvoiceStatus.Sent;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkViewed()
    {
        if (Status != InvoiceStatus.Sent)
            throw new InvalidOperationException($"Cannot mark as viewed in status {Status}. Must be Sent.");

        Status = InvoiceStatus.Viewed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordPayment(decimal amount)
    {
        if (Status == InvoiceStatus.Paid || Status == InvoiceStatus.Voided)
            throw new InvalidOperationException($"Cannot record payment for invoice in status {Status}.");

        PaidAmount += amount;

        if (PaidAmount >= TotalAmount)
            Status = InvoiceStatus.Paid;
        else
            Status = InvoiceStatus.PartiallyPaid;

        UpdatedAt = DateTime.UtcNow;
    }

    public void Void()
    {
        if (Status == InvoiceStatus.Paid)
            throw new InvalidOperationException("Cannot void a fully paid invoice.");

        Status = InvoiceStatus.Voided;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkOverdue()
    {
        if (Status == InvoiceStatus.Paid || Status == InvoiceStatus.Voided)
            throw new InvalidOperationException($"Cannot mark overdue in status {Status}.");

        Status = InvoiceStatus.Overdue;
        UpdatedAt = DateTime.UtcNow;
    }
}
