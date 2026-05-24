using TheLeague.Shared.Domain.Enums;

namespace TheLeague.Modules.Events.Application.Dtos;

public record EventDto(
    Guid Id,
    Guid ClubId,
    string Title,
    string? Description,
    EventType EventType,
    EventStatus Status,
    DateTime StartDateTime,
    DateTime EndDateTime,
    Guid? VenueId,
    string? VenueName,
    int? Capacity,
    int CurrentRegistrationCount,
    bool IsTicketed,
    decimal? StandardPrice,
    decimal? MemberPrice,
    bool AllowRsvp,
    int CancellationDeadlineHours,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record EventRegistrationDto(
    Guid Id,
    Guid EventId,
    Guid MemberId,
    string RegistrationType,
    DateTime RegisteredAt,
    DateTime? CancelledAt,
    bool RefundInitiated);

public record EventTicketDto(
    Guid Id,
    Guid EventId,
    Guid MemberId,
    string TicketNumber,
    string QRCodeData,
    decimal PricePaid,
    DateTime PurchasedAt,
    bool IsCheckedIn,
    DateTime? CheckedInAt);

public record EventDetailDto(
    Guid Id,
    Guid ClubId,
    string Title,
    string? Description,
    EventType EventType,
    EventStatus Status,
    DateTime StartDateTime,
    DateTime EndDateTime,
    Guid? VenueId,
    string? VenueName,
    int? Capacity,
    int CurrentRegistrationCount,
    bool IsTicketed,
    decimal? StandardPrice,
    decimal? MemberPrice,
    bool AllowRsvp,
    int CancellationDeadlineHours,
    List<EventRegistrationDto> Registrations,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
