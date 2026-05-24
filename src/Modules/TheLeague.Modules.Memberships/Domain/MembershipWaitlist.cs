using TheLeague.Shared.Domain.Entities;

namespace TheLeague.Modules.Memberships.Domain;

public class MembershipWaitlist : TenantEntity
{
    public Guid MembershipTypeId { get; private set; }
    public Guid MemberId { get; private set; }
    public int Position { get; private set; }
    public DateTime RequestedAt { get; private set; }
    public DateTime? NotifiedAt { get; private set; }
    public string Status { get; private set; } = WaitlistStatus.Waiting;

    public static MembershipWaitlist Create(
        Guid clubId,
        Guid membershipTypeId,
        Guid memberId,
        int position)
    {
        return new MembershipWaitlist
        {
            ClubId = clubId,
            MembershipTypeId = membershipTypeId,
            MemberId = memberId,
            Position = position,
            RequestedAt = DateTime.UtcNow,
            Status = WaitlistStatus.Waiting
        };
    }

    public void MarkNotified()
    {
        NotifiedAt = DateTime.UtcNow;
        Status = WaitlistStatus.Offered;
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
