using TheLeague.Shared.Domain.Entities;
using TheLeague.Shared.Domain.Enums;

namespace TheLeague.Modules.Payments.Domain;

public class ChartOfAccount : TenantEntity
{
    public string AccountNumber { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public AccountCategory Category { get; private set; }
    public Guid? ParentAccountId { get; private set; }
    public bool IsActive { get; private set; }

    private ChartOfAccount() { }

    public static ChartOfAccount Create(
        Guid clubId,
        string accountNumber,
        string name,
        AccountCategory category,
        Guid? parentAccountId = null)
    {
        return new ChartOfAccount
        {
            ClubId = clubId,
            AccountNumber = accountNumber,
            Name = name,
            Category = category,
            ParentAccountId = parentAccountId,
            IsActive = true
        };
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
