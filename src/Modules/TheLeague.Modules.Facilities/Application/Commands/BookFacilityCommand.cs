using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Facilities.Application.Dtos;
using TheLeague.Modules.Facilities.Application.Services;
using TheLeague.Modules.Facilities.Domain;
using TheLeague.Modules.Facilities.Infrastructure.Persistence;
using TheLeague.Shared.Contracts.Events;
using TheLeague.Shared.Contracts.Messaging;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Facilities.Application.Commands;

public record BookFacilityCommand(
    Guid FacilityId,
    Guid MemberId,
    DateOnly BookingDate,
    TimeOnly StartTime,
    int Duration,
    bool IsMember
) : IRequest<Result<FacilityBookingDto>>;

public class BookFacilityCommandHandler : IRequestHandler<BookFacilityCommand, Result<FacilityBookingDto>>
{
    private readonly FacilitiesDbContext _db;
    private readonly ITenantService _tenantService;
    private readonly ConflictDetectionService _conflictService;
    private readonly IIntegrationEventBus _eventBus;

    public BookFacilityCommandHandler(
        FacilitiesDbContext db,
        ITenantService tenantService,
        ConflictDetectionService conflictService,
        IIntegrationEventBus eventBus)
    {
        _db = db;
        _tenantService = tenantService;
        _conflictService = conflictService;
        _eventBus = eventBus;
    }

    public async Task<Result<FacilityBookingDto>> Handle(BookFacilityCommand request, CancellationToken cancellationToken)
    {
        var clubId = _tenantService.CurrentTenantId;
        if (clubId == null)
            return Result.Failure<FacilityBookingDto>("Tenant context is required.");

        // Validate duration (30-240 minutes in 30-min increments)
        if (request.Duration < 30 || request.Duration > 240 || request.Duration % 30 != 0)
            return Result.Failure<FacilityBookingDto>("Duration must be between 30 and 240 minutes in 30-minute increments.");

        // Validate advance booking limit (max 30 days)
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (request.BookingDate > today.AddDays(30))
            return Result.Failure<FacilityBookingDto>("Bookings cannot be made more than 30 days in advance.");

        if (request.BookingDate < today)
            return Result.Failure<FacilityBookingDto>("Cannot book a date in the past.");

        // Check facility exists and is active
        var facility = await _db.Facilities.FirstOrDefaultAsync(f => f.Id == request.FacilityId, cancellationToken);
        if (facility == null)
            return Result.Failure<FacilityBookingDto>("Facility not found.");

        if (!facility.IsActive)
            return Result.Failure<FacilityBookingDto>("Facility is not currently active.");

        var endTime = request.StartTime.AddMinutes(request.Duration);

        // Check availability (operating hours)
        var availability = await _db.FacilityAvailabilities
            .Where(a => a.FacilityId == request.FacilityId
                && a.DayOfWeek == request.BookingDate.DayOfWeek
                && a.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        if (availability != null)
        {
            if (request.StartTime < availability.OpenTime || endTime > availability.CloseTime)
                return Result.Failure<FacilityBookingDto>("Booking time is outside operating hours.");
        }

        // Conflict detection
        var conflictResult = await _conflictService.CheckConflictsAsync(
            request.FacilityId, request.BookingDate, request.StartTime, endTime, cancellationToken);

        if (conflictResult.HasConflict)
        {
            var message = conflictResult.Reason!;
            if (conflictResult.Alternatives.Any())
            {
                var altSlots = string.Join(", ", conflictResult.Alternatives.Select(a => $"{a.StartTime:HH:mm}-{a.EndTime:HH:mm}"));
                message += $" Alternative slots available: {altSlots}";
            }
            return Result.Failure<FacilityBookingDto>(message);
        }

        // Apply pricing
        var price = await CalculatePriceAsync(request.FacilityId, request.StartTime, endTime, request.IsMember, cancellationToken);

        // Create booking
        var booking = FacilityBooking.Create(
            clubId.Value,
            request.FacilityId,
            request.MemberId,
            request.BookingDate,
            request.StartTime,
            request.Duration,
            request.IsMember,
            price);

        _db.FacilityBookings.Add(booking);
        await _db.SaveChangesAsync(cancellationToken);

        // Publish integration event
        await _eventBus.PublishAsync(
            new FacilityBookedEvent(booking.Id, booking.FacilityId, booking.MemberId, clubId.Value),
            cancellationToken);

        var dto = new FacilityBookingDto(
            booking.Id, booking.FacilityId, booking.MemberId,
            booking.BookingDate, booking.StartTime, booking.EndTime,
            booking.Duration, booking.IsMember, booking.PricePaid,
            booking.Status, booking.BookingReference, booking.BookedAt);

        return Result.Success(dto);
    }

    private async Task<decimal> CalculatePriceAsync(
        Guid facilityId, TimeOnly startTime, TimeOnly endTime, bool isMember, CancellationToken ct)
    {
        var pricings = await _db.FacilityPricings
            .Where(p => p.FacilityId == facilityId)
            .ToListAsync(ct);

        if (!pricings.Any())
            return 0m;

        // Check if the booking falls within peak hours
        var peakPricing = pricings.FirstOrDefault(p => p.IsPeakRate
            && p.PeakStartTime.HasValue && p.PeakEndTime.HasValue
            && startTime >= p.PeakStartTime.Value && endTime <= p.PeakEndTime.Value);

        if (peakPricing != null)
            return isMember ? peakPricing.MemberRate : peakPricing.NonMemberRate;

        // Use off-peak rate
        var offPeakPricing = pricings.FirstOrDefault(p => !p.IsPeakRate);
        if (offPeakPricing != null)
            return isMember ? offPeakPricing.MemberRate : offPeakPricing.NonMemberRate;

        return 0m;
    }
}
