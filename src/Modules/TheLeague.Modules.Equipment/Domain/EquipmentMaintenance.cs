using TheLeague.Shared.Domain.Entities;
using TheLeague.Shared.Domain.Enums;

namespace TheLeague.Modules.Equipment.Domain;

public class EquipmentMaintenance : TenantEntity
{
    public Guid EquipmentId { get; private set; }
    public DateTime MaintenanceDate { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public EquipmentCondition ResultingCondition { get; private set; }
    public decimal? Cost { get; private set; }
    public string? PerformedBy { get; private set; }

    // Navigation
    public EquipmentItem Equipment { get; private set; } = null!;

    public static EquipmentMaintenance Create(
        Guid clubId,
        Guid equipmentId,
        DateTime maintenanceDate,
        string description,
        EquipmentCondition resultingCondition,
        decimal? cost,
        string? performedBy)
    {
        return new EquipmentMaintenance
        {
            ClubId = clubId,
            EquipmentId = equipmentId,
            MaintenanceDate = maintenanceDate,
            Description = description,
            ResultingCondition = resultingCondition,
            Cost = cost,
            PerformedBy = performedBy
        };
    }
}
