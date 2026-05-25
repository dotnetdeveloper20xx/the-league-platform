using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Facilities.Domain;
using TheLeague.Modules.Facilities.Infrastructure.Persistence;

namespace TheLeague.Modules.Facilities.Application.Services;

public record ConflictResult(bool HasConflict, string? Reason, List<AlternativeSlot> Alternatives);
public record AlternativeSlot(TimeOnly StartTime, TimeOnly EndTime);

public class ConflictDetectionService
{
    private readonly FacilitiesDbContext _db;

    public ConflictDetectionService(FacilitiesDbContext db)
    {
        _db = db;
    }

    public async Task<ConflictResult> CheckConflictsAsync(
        Guid facilityId,
        DateOnly bookingDate,
        TimeOnly startTime,
        TimeOnly endTime,
        CancellationToken ct)
    {
        // Check overlap with existing confirmed bookings
        var hasBookingConflict = await _db.FacilityBookings
            .Where(b => b.FacilityId == facilityId
                && b.BookingDate == bookingDate
                && b.Status == BookingStatus.Confirmed
                && b.StartTime < endTime
                && b.EndTime > startTime)
            .AnyAsync(ct);

        if (hasBookingConflict)
        {
            var alternatives = await SuggestAlternativesAsync(facilityId, bookingDate, startTime, endTime, ct);
            return new ConflictResult(true, "Time slot conflicts with an existing booking.", alternatives);
        }

        // Check overlap with maintenance windows
        var bookingStart = bookingDate.ToDateTime(startTime);
        var bookingEnd = bookingDate.ToDateTime(endTime);

        var hasMaintenanceConflict = await _db.FacilityMaintenances
            .Where(m => m.FacilityId == facilityId
                && !m.IsCompleted
                && m.StartDate < bookingEnd
                && m.EndDate > bookingStart)
            .AnyAsync(ct);

        if (hasMaintenanceConflict)
        {
            var alternatives = await SuggestAlternativesAsync(facilityId, bookingDate, startTime, endTime, ct);
            return new ConflictResult(true, "Time slot conflicts with a scheduled maintenance window.", alternatives);
        }

        // Check overlap with blockout periods
        var hasBlockoutConflict = await _db.FacilityBlockouts
            .Where(b => b.FacilityId == facilityId
                && b.StartDate < bookingEnd
                && b.EndDate > bookingStart)
            .AnyAsync(ct);

        if (hasBlockoutConflict)
        {
            var alternatives = await SuggestAlternativesAsync(facilityId, bookingDate, startTime, endTime, ct);
            return new ConflictResult(true, "Time slot conflicts with a blockout period.", alternatives);
        }

        return new ConflictResult(false, null, new List<AlternativeSlot>());
    }

    private async Task<List<AlternativeSlot>> SuggestAlternativesAsync(
        Guid facilityId,
        DateOnly bookingDate,
        TimeOnly requestedStart,
        TimeOnly requestedEnd,
        CancellationToken ct)
    {
        var duration = (int)(requestedEnd - requestedStart).TotalMinutes;

        // Get operating hours for this day
        var availability = await _db.FacilityAvailabilities
            .Where(a => a.FacilityId == facilityId
                && a.DayOfWeek == bookingDate.DayOfWeek
                && a.IsActive)
            .FirstOrDefaultAsync(ct);

        if (availability == null)
            return new List<AlternativeSlot>();

        // Get all existing bookings for the day
        var existingBookings = await _db.FacilityBookings
            .Where(b => b.FacilityId == facilityId
                && b.BookingDate == bookingDate
                && b.Status == BookingStatus.Confirmed)
            .OrderBy(b => b.StartTime)
            .Select(b => new { b.StartTime, b.EndTime })
            .ToListAsync(ct);

        // Get maintenance windows for the day
        var dayStart = bookingDate.ToDateTime(TimeOnly.MinValue);
        var dayEnd = bookingDate.ToDateTime(TimeOnly.MaxValue);

        var maintenanceWindows = await _db.FacilityMaintenances
            .Where(m => m.FacilityId == facilityId
                && !m.IsCompleted
                && m.StartDate < dayEnd
                && m.EndDate > dayStart)
            .Select(m => new { m.StartDate, m.EndDate })
            .ToListAsync(ct);

        var blockouts = await _db.FacilityBlockouts
            .Where(b => b.FacilityId == facilityId
                && b.StartDate < dayEnd
                && b.EndDate > dayStart)
            .Select(b => new { b.StartDate, b.EndDate })
            .ToListAsync(ct);

        var alternatives = new List<AlternativeSlot>();
        var currentSlot = availability.OpenTime;

        while (currentSlot.AddMinutes(duration) <= availability.CloseTime && alternatives.Count < 3)
        {
            var slotEnd = currentSlot.AddMinutes(duration);
            var slotStart = currentSlot;

            // Skip the originally requested slot
            if (slotStart == requestedStart)
            {
                currentSlot = currentSlot.AddMinutes(30);
                continue;
            }

            var slotStartDt = bookingDate.ToDateTime(slotStart);
            var slotEndDt = bookingDate.ToDateTime(slotEnd);

            // Check no booking conflict
            var bookingConflict = existingBookings.Any(b => b.StartTime < slotEnd && b.EndTime > slotStart);

            // Check no maintenance conflict
            var maintenanceConflict = maintenanceWindows.Any(m => m.StartDate < slotEndDt && m.EndDate > slotStartDt);

            // Check no blockout conflict
            var blockoutConflict = blockouts.Any(b => b.StartDate < slotEndDt && b.EndDate > slotStartDt);

            if (!bookingConflict && !maintenanceConflict && !blockoutConflict)
            {
                alternatives.Add(new AlternativeSlot(slotStart, slotEnd));
            }

            currentSlot = currentSlot.AddMinutes(30);
        }

        return alternatives;
    }
}
