using TheLeague.Shared.Domain.Entities;
using TheLeague.Shared.Domain.Enums;

namespace TheLeague.Modules.Memberships.Domain;

public class Membership : TenantEntity
{
    public Guid MemberId { get; private set; }
    public Guid MembershipTypeId { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public MembershipStatus Status { get; private set; } = MembershipStatus.Active;
    public bool AutoRenew { get; private set; }
    public decimal PricePaid { get; private set; }
    public decimal? DiscountApplied { get; private set; }
    public DiscountType? DiscountType { get; private set; }

    public static Membership Create(
        Guid clubId,
        Guid memberId,
        Guid membershipTypeId,
        DateTime startDate,
        DateTime endDate,
        bool autoRenew,
        decimal pricePaid,
        decimal? discountApplied = null,
        DiscountType? discountType = null)
    {
        return new Membership
        {
            ClubId = clubId,
            MemberId = memberId,
            MembershipTypeId = membershipTypeId,
            StartDate = startDate,
            EndDate = endDate,
            AutoRenew = autoRenew,
            PricePaid = pricePaid,
            DiscountApplied = discountApplied,
            DiscountType = discountType,
            Status = MembershipStatus.Active
        };
    }

    public void Renew(DateTime newEndDate)
    {
        if (Status != MembershipStatus.Active && Status != MembershipStatus.Expired)
            throw new InvalidOperationException($"Cannot renew membership in {Status} status.");

        StartDate = EndDate;
        EndDate = newEndDate;
        Status = MembershipStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Freeze()
    {
        if (Status != MembershipStatus.Active)
            throw new InvalidOperationException($"Cannot freeze membership in {Status} status. Only Active memberships can be frozen.");

        // Freeze is tracked via MembershipFreeze record; status remains Active
        // but the membership is effectively paused
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == MembershipStatus.Cancelled)
            throw new InvalidOperationException("Membership is already cancelled.");

        Status = MembershipStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Expire()
    {
        if (Status != MembershipStatus.Active)
            throw new InvalidOperationException($"Cannot expire membership in {Status} status.");

        Status = MembershipStatus.Expired;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetPendingPayment()
    {
        if (Status != MembershipStatus.Active && Status != MembershipStatus.PendingPayment)
            throw new InvalidOperationException($"Cannot set pending payment for membership in {Status} status.");

        Status = MembershipStatus.PendingPayment;
        UpdatedAt = DateTime.UtcNow;
    }
}
