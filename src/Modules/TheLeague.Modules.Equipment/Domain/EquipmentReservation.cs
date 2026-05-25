using TheLeague.Shared.Domain.Entities;

namespace TheLeague.Modules.Equipment.Domain;

public enum ReservationStatus
{
    Pending,
    Approved,
    Cancelled
}

public class EquipmentReservation : TenantEntity
{
    public Guid EquipmentId { get; private set; }
    public Guid MemberId { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public ReservationStatus Status { get; private set; }
    public string? Notes { get; private set; }

    // Navigation
    public EquipmentItem Equipment { get; private set; } = null!;

    public static EquipmentReservation Create(
        Guid clubId,
        Guid equipmentId,
        Guid memberId,
        DateTime startDate,
        DateTime endDate,
        string? notes)
    {
        // Max 365 days advance
        var maxEndDate = DateTime.UtcNow.AddDays(365);
        if (endDate > maxEndDate)
            endDate = maxEndDate;

        return new EquipmentReservation
        {
            ClubId = clubId,
            EquipmentId = equipmentId,
            MemberId = memberId,
            StartDate = startDate,
            EndDate = endDate,
            Status = ReservationStatus.Pending,
            Notes = notes
        };
    }

    public void Approve()
    {
        Status = ReservationStatus.Approved;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        Status = ReservationStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool OverlapsWith(DateTime startDate, DateTime endDate)
    {
        if (Status == ReservationStatus.Cancelled)
            return false;

        return StartDate < endDate && EndDate > startDate;
    }
}
