using TheLeague.Shared.Domain.Entities;

namespace TheLeague.Modules.Sessions.Domain;

public class Waitlist : TenantEntity
{
    public Guid SessionId { get; private set; }
    public Guid MemberId { get; private set; }
    public int Position { get; private set; }
    public DateTime RequestedAt { get; private set; }
    public DateTime? OfferedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public string Status { get; private set; } = WaitlistStatus.Waiting;

    // Navigation
    public Session? Session { get; private set; }

    public static Waitlist Create(Guid clubId, Guid sessionId, Guid memberId, int position)
    {
        return new Waitlist
        {
            ClubId = clubId,
            SessionId = sessionId,
            MemberId = memberId,
            Position = position,
            RequestedAt = DateTime.UtcNow,
            Status = WaitlistStatus.Waiting
        };
    }

    public void Offer()
    {
        Status = WaitlistStatus.Offered;
        OfferedAt = DateTime.UtcNow;
        ExpiresAt = DateTime.UtcNow.AddHours(24);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Accept()
    {
        Status = WaitlistStatus.Accepted;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Expire()
    {
        Status = WaitlistStatus.Expired;
        UpdatedAt = DateTime.UtcNow;
    }
}

public static class WaitlistStatus
{
    public const string Waiting = "Waiting";
    public const string Offered = "Offered";
    public const string Accepted = "Accepted";
    public const string Expired = "Expired";
}
