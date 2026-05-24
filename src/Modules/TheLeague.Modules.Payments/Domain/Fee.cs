using TheLeague.Shared.Domain.Entities;
using TheLeague.Shared.Domain.Enums;

namespace TheLeague.Modules.Payments.Domain;

public class Fee : TenantEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public FeeType FeeType { get; private set; }
    public decimal Amount { get; private set; }
    public bool IsRecurring { get; private set; }
    public string? BillingCycle { get; private set; }
    public bool IsActive { get; private set; }

    private Fee() { }

    public static Fee Create(
        Guid clubId,
        string name,
        string code,
        FeeType feeType,
        decimal amount,
        bool isRecurring = false,
        string? billingCycle = null)
    {
        return new Fee
        {
            ClubId = clubId,
            Name = name,
            Code = code,
            FeeType = feeType,
            Amount = amount,
            IsRecurring = isRecurring,
            BillingCycle = billingCycle,
            IsActive = true
        };
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
