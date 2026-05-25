using TheLeague.Modules.Facilities.Domain;

namespace TheLeague.Modules.Facilities.Application.Dtos;

public record FacilityDto(
    Guid Id,
    string Name,
    FacilityType FacilityType,
    string? Description,
    int? Capacity,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record FacilityBookingDto(
    Guid Id,
    Guid FacilityId,
    Guid MemberId,
    DateOnly BookingDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int Duration,
    bool IsMember,
    decimal PricePaid,
    BookingStatus Status,
    string BookingReference,
    DateTime BookedAt);

public record FacilityAvailabilitySlotDto(
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsAvailable);

public record FacilityAvailabilityResponseDto(
    Guid FacilityId,
    DateOnly Date,
    List<FacilityAvailabilitySlotDto> Slots);
