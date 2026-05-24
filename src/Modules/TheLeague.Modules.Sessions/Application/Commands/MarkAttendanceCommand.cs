using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Sessions.Infrastructure.Persistence;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Enums;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Sessions.Application.Commands;

public record MarkAttendanceCommand(
    Guid SessionId,
    List<AttendanceEntry> Entries
) : IRequest<Result>;

public record AttendanceEntry(Guid BookingId, BookingStatus Status);

public class MarkAttendanceCommandHandler : IRequestHandler<MarkAttendanceCommand, Result>
{
    private readonly SessionsDbContext _db;
    private readonly ITenantService _tenantService;

    public MarkAttendanceCommandHandler(SessionsDbContext db, ITenantService tenantService)
    {
        _db = db;
        _tenantService = tenantService;
    }

    public async Task<Result> Handle(MarkAttendanceCommand request, CancellationToken cancellationToken)
    {
        var clubId = _tenantService.CurrentTenantId;
        if (clubId is null)
            return Result.Failure("Tenant context is required.");

        var session = await _db.Sessions.FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);
        if (session is null)
            return Result.Failure("Session not found.");

        var bookingIds = request.Entries.Select(e => e.BookingId).ToList();
        var bookings = await _db.SessionBookings
            .Where(b => b.SessionId == request.SessionId && bookingIds.Contains(b.Id))
            .ToListAsync(cancellationToken);

        foreach (var entry in request.Entries)
        {
            var booking = bookings.FirstOrDefault(b => b.Id == entry.BookingId);
            if (booking is null) continue;

            switch (entry.Status)
            {
                case BookingStatus.Attended:
                    booking.MarkAttended();
                    break;
                case BookingStatus.NoShow:
                    booking.MarkNoShow();
                    break;
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success("Attendance marked successfully.");
    }
}
