using TheLeague.Shared.Domain.Entities;
using TheLeague.Shared.Domain.Enums;

namespace TheLeague.Modules.Sessions.Domain;

public class SessionBooking : TenantEntity
{
    public Guid SessionId { get; private set; }
    public Guid MemberId { get; private set; }
    public BookingStatus Status { get; private set; }
    public DateTime BookedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }

    // Navigation
    public Session? Session { get; private set; }

    public static SessionBooking Create(Guid clubId, Guid sessionId, Guid memberId)
    {
        return new SessionBooking
        {
            ClubId = clubId,
            SessionId = sessionId,
            MemberId = memberId,
            Status = BookingStatus.Confirmed,
            BookedAt = DateTime.UtcNow
        };
    }

    public void Confirm()
    {
        Status = BookingStatus.Confirmed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        Status = BookingStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAttended()
    {
        Status = BookingStatus.Attended;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkNoShow()
    {
        Status = BookingStatus.NoShow;
        UpdatedAt = DateTime.UtcNow;
    }
}
