using TheLeague.Shared.Domain.Entities;

namespace TheLeague.Modules.Memberships.Domain;

public class MembershipFreeze : TenantEntity
{
    public Guid MembershipId { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public decimal FeeCharged { get; private set; }
    public string? Reason { get; private set; }

    public static MembershipFreeze Create(
        Guid clubId,
        Guid membershipId,
        DateTime startDate,
        DateTime endDate,
        decimal feeCharged,
        string? reason = null)
    {
        return new MembershipFreeze
        {
            ClubId = clubId,
            MembershipId = membershipId,
            StartDate = startDate,
            EndDate = endDate,
            FeeCharged = feeCharged,
            Reason = reason
        };
    }
}
