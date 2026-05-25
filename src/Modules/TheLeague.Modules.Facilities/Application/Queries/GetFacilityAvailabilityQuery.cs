using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Facilities.Application.Dtos;
using TheLeague.Modules.Facilities.Domain;
using TheLeague.Modules.Facilities.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Facilities.Application.Queries;

public record GetFacilityAvailabilityQuery(Guid FacilityId, DateOnly Date) : IRequest<Result<FacilityAvailabilityResponseDto>>;

public class GetFacilityAvailabilityQueryHandler : IRequestHandler<GetFacilityAvailabilityQuery, Result<FacilityAvailabilityResponseDto>>
{
    private readonly FacilitiesDbContext _db;

    public GetFacilityAvailabilityQueryHandler(FacilitiesDbContext db)
    {
        _db = db;
    }

    public async Task<Result<FacilityAvailabilityResponseDto>> Handle(GetFacilityAvailabilityQuery request, CancellationToken cancellationToken)
    {
        var facility = await _db.Facilities
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == request.FacilityId, cancellationToken);

        if (facility == null)
            return Result.Failure<FacilityAvailabilityResponseDto>("Facility not found.");

        // Get operating hours for the requested day
        var availability = await _db.FacilityAvailabilities
            .AsNoTracking()
            .Where(a => a.FacilityId == request.FacilityId
                && a.DayOfWeek == request.Date.DayOfWeek
                && a.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        if (availability == null)
        {
            return Result.Success(new FacilityAvailabilityResponseDto(
                request.FacilityId, request.Date, new List<FacilityAvailabilitySlotDto>()));
        }

        // Get existing bookings for the day
        var existingBookings = await _db.FacilityBookings
            .AsNoTracking()
            .Where(b => b.FacilityId == request.FacilityId
                && b.BookingDate == request.Date
                && b.Status == BookingStatus.Confirmed)
            .Select(b => new { b.StartTime, b.EndTime })
            .ToListAsync(cancellationToken);

        // Get maintenance windows for the day
        var dayStart = request.Date.ToDateTime(TimeOnly.MinValue);
        var dayEnd = request.Date.ToDateTime(TimeOnly.MaxValue);

        var maintenanceWindows = await _db.FacilityMaintenances
            .AsNoTracking()
            .Where(m => m.FacilityId == request.FacilityId
                && !m.IsCompleted
                && m.StartDate < dayEnd
                && m.EndDate > dayStart)
            .Select(m => new { m.StartDate, m.EndDate })
            .ToListAsync(cancellationToken);

        var blockouts = await _db.FacilityBlockouts
            .AsNoTracking()
            .Where(b => b.FacilityId == request.FacilityId
                && b.StartDate < dayEnd
                && b.EndDate > dayStart)
            .Select(b => new { b.StartDate, b.EndDate })
            .ToListAsync(cancellationToken);

        // Generate 30-minute slots
        var slots = new List<FacilityAvailabilitySlotDto>();
        var currentSlot = availability.OpenTime;

        while (currentSlot.AddMinutes(30) <= availability.CloseTime)
        {
            var slotEnd = currentSlot.AddMinutes(30);
            var slotStartDt = request.Date.ToDateTime(currentSlot);
            var slotEndDt = request.Date.ToDateTime(slotEnd);

            var isBooked = existingBookings.Any(b => b.StartTime < slotEnd && b.EndTime > currentSlot);
            var hasMaintenance = maintenanceWindows.Any(m => m.StartDate < slotEndDt && m.EndDate > slotStartDt);
            var hasBlockout = blockouts.Any(b => b.StartDate < slotEndDt && b.EndDate > slotStartDt);

            var isAvailable = !isBooked && !hasMaintenance && !hasBlockout;

            slots.Add(new FacilityAvailabilitySlotDto(currentSlot, slotEnd, isAvailable));
            currentSlot = slotEnd;
        }

        var response = new FacilityAvailabilityResponseDto(request.FacilityId, request.Date, slots);
        return Result.Success(response);
    }
}
