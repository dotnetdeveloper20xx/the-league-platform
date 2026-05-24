using TheLeague.Shared.Domain.Enums;

namespace TheLeague.Modules.Sessions.Application.Dtos;

public record SessionDto(
    Guid Id,
    string Title,
    SessionCategory Category,
    Guid? VenueId,
    string? VenueName,
    DateTime StartTime,
    DateTime EndTime,
    int Duration,
    int Capacity,
    decimal Fee,
    int CurrentBookingCount,
    int RemainingCapacity,
    bool IsCancelled,
    string? CancellationReason,
    int CancellationDeadlineHours,
    DateTime CreatedAt);

public record SessionDetailDto(
    Guid Id,
    string Title,
    SessionCategory Category,
    Guid? VenueId,
    string? VenueName,
    DateTime StartTime,
    DateTime EndTime,
    int Duration,
    int Capacity,
    decimal Fee,
    int CurrentBookingCount,
    int RemainingCapacity,
    bool IsCancelled,
    string? CancellationReason,
    int CancellationDeadlineHours,
    DateTime CreatedAt,
    List<SessionBookingDto> Bookings);

public record SessionBookingDto(
    Guid Id,
    Guid SessionId,
    Guid MemberId,
    BookingStatus Status,
    DateTime BookedAt,
    DateTime? CancelledAt);

public record RecurringScheduleDto(
    Guid Id,
    string Title,
    SessionCategory Category,
    Guid? VenueId,
    string? VenueName,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    int Duration,
    int Capacity,
    decimal Fee,
    int HorizonWeeks,
    bool IsActive,
    DateTime CreatedAt);

public record WaitlistDto(
    Guid Id,
    Guid SessionId,
    Guid MemberId,
    int Position,
    DateTime RequestedAt,
    DateTime? OfferedAt,
    DateTime? ExpiresAt,
    string Status);
