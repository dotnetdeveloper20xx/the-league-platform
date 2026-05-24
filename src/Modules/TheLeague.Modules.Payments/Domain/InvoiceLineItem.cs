using TheLeague.Shared.Domain.Entities;
using TheLeague.Shared.Domain.Enums;

namespace TheLeague.Modules.Payments.Domain;

public class InvoiceLineItem : TenantEntity
{
    public Guid InvoiceId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TotalPrice { get; private set; }
    public FeeType FeeType { get; private set; }

    private InvoiceLineItem() { }

    public static InvoiceLineItem Create(
        Guid clubId,
        Guid invoiceId,
        string description,
        int quantity,
        decimal unitPrice,
        FeeType feeType)
    {
        return new InvoiceLineItem
        {
            ClubId = clubId,
            InvoiceId = invoiceId,
            Description = description,
            Quantity = quantity,
            UnitPrice = unitPrice,
            TotalPrice = quantity * unitPrice,
            FeeType = feeType
        };
    }
}
