using TheLeague.Shared.Domain.Entities;

namespace TheLeague.Modules.Payments.Domain;

public class MemberBalance : TenantEntity
{
    public Guid MemberId { get; private set; }
    public decimal CreditBalance { get; private set; }
    public decimal OutstandingBalance { get; private set; }
    public DateTime LastUpdated { get; private set; }

    private MemberBalance() { }

    public static MemberBalance Create(Guid clubId, Guid memberId)
    {
        return new MemberBalance
        {
            ClubId = clubId,
            MemberId = memberId,
            CreditBalance = 0,
            OutstandingBalance = 0,
            LastUpdated = DateTime.UtcNow
        };
    }

    public void AddCredit(decimal amount)
    {
        CreditBalance += amount;
        LastUpdated = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddDebit(decimal amount)
    {
        OutstandingBalance += amount;
        LastUpdated = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void DeductCredit(decimal amount)
    {
        CreditBalance -= amount;
        LastUpdated = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReduceOutstanding(decimal amount)
    {
        OutstandingBalance -= amount;
        LastUpdated = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
