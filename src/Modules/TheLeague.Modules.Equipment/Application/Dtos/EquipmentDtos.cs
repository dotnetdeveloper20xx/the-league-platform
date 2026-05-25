using TheLeague.Modules.Equipment.Domain;
using TheLeague.Shared.Domain.Enums;

namespace TheLeague.Modules.Equipment.Application.Dtos;

public record EquipmentDto(
    Guid Id,
    string Name,
    EquipmentCategory Category,
    EquipmentCondition Condition,
    string Location,
    DateTime? PurchaseDate,
    decimal Value,
    decimal AnnualDepreciationRate,
    string? SerialNumber,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record EquipmentDetailDto(
    Guid Id,
    string Name,
    EquipmentCategory Category,
    EquipmentCondition Condition,
    string Location,
    DateTime? PurchaseDate,
    decimal Value,
    decimal AnnualDepreciationRate,
    string? SerialNumber,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<LoanDto> Loans,
    List<ReservationDto> Reservations,
    List<MaintenanceDto> MaintenanceRecords);

public record LoanDto(
    Guid Id,
    Guid EquipmentId,
    Guid MemberId,
    LoanStatus Status,
    DateTime LoanDate,
    DateTime ExpectedReturnDate,
    DateTime? ActualReturnDate,
    decimal Fee,
    decimal Deposit,
    string? Notes,
    DateTime CreatedAt);

public record ReservationDto(
    Guid Id,
    Guid EquipmentId,
    Guid MemberId,
    DateTime StartDate,
    DateTime EndDate,
    ReservationStatus Status,
    string? Notes,
    DateTime CreatedAt);

public record MaintenanceDto(
    Guid Id,
    Guid EquipmentId,
    DateTime MaintenanceDate,
    string Description,
    EquipmentCondition ResultingCondition,
    decimal? Cost,
    string? PerformedBy,
    DateTime CreatedAt);
