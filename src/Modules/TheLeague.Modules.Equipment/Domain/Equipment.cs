using TheLeague.Shared.Domain.Entities;
using TheLeague.Shared.Domain.Enums;

namespace TheLeague.Modules.Equipment.Domain;

public class EquipmentItem : TenantEntity
{
    public string Name { get; private set; } = string.Empty;
    public EquipmentCategory Category { get; private set; }
    public EquipmentCondition Condition { get; private set; }
    public string Location { get; private set; } = string.Empty;
    public DateTime? PurchaseDate { get; private set; }
    public decimal Value { get; private set; }
    public decimal AnnualDepreciationRate { get; private set; }
    public string? SerialNumber { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Navigation properties
    public ICollection<EquipmentLoan> Loans { get; private set; } = new List<EquipmentLoan>();
    public ICollection<EquipmentReservation> Reservations { get; private set; } = new List<EquipmentReservation>();
    public ICollection<EquipmentMaintenance> MaintenanceRecords { get; private set; } = new List<EquipmentMaintenance>();

    public static EquipmentItem Create(
        Guid clubId,
        string name,
        EquipmentCategory category,
        EquipmentCondition condition,
        string location,
        DateTime? purchaseDate,
        decimal value,
        decimal annualDepreciationRate,
        string? serialNumber)
    {
        return new EquipmentItem
        {
            ClubId = clubId,
            Name = name,
            Category = category,
            Condition = condition,
            Location = location,
            PurchaseDate = purchaseDate,
            Value = value,
            AnnualDepreciationRate = Math.Clamp(annualDepreciationRate, 0, 100),
            SerialNumber = serialNumber
        };
    }

    public void Update(
        string name,
        EquipmentCategory category,
        EquipmentCondition condition,
        string location,
        DateTime? purchaseDate,
        decimal value,
        decimal annualDepreciationRate,
        string? serialNumber,
        bool isActive)
    {
        Name = name;
        Category = category;
        Condition = condition;
        Location = location;
        PurchaseDate = purchaseDate;
        Value = value;
        AnnualDepreciationRate = Math.Clamp(annualDepreciationRate, 0, 100);
        SerialNumber = serialNumber;
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateCondition(EquipmentCondition condition)
    {
        Condition = condition;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsLoanable()
    {
        return Condition != EquipmentCondition.NeedsRepair
            && Condition != EquipmentCondition.Damaged
            && Condition != EquipmentCondition.Decommissioned
            && IsActive;
    }
}
