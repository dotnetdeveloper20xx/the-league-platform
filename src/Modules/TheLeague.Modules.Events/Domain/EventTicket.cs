using TheLeague.Shared.Domain.Entities;

namespace TheLeague.Modules.Events.Domain;

public class EventTicket : TenantEntity
{
    public Guid EventId { get; private set; }
    public Guid MemberId { get; private set; }
    public string TicketNumber { get; private set; } = string.Empty;
    public string QRCodeData { get; private set; } = string.Empty;
    public decimal PricePaid { get; private set; }
    public DateTime PurchasedAt { get; private set; }
    public bool IsCheckedIn { get; private set; }
    public DateTime? CheckedInAt { get; private set; }

    private EventTicket() { }

    public static EventTicket Create(
        Guid clubId,
        Guid eventId,
        Guid memberId,
        string ticketNumber,
        string qrCodeData,
        decimal pricePaid)
    {
        return new EventTicket
        {
            ClubId = clubId,
            EventId = eventId,
            MemberId = memberId,
            TicketNumber = ticketNumber,
            QRCodeData = qrCodeData,
            PricePaid = pricePaid,
            PurchasedAt = DateTime.UtcNow
        };
    }

    public void CheckIn()
    {
        if (IsCheckedIn)
            throw new InvalidOperationException("Ticket has already been checked in.");

        IsCheckedIn = true;
        CheckedInAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
