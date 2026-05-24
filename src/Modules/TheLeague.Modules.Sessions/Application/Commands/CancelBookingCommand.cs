using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Sessions.Domain;
using TheLeague.Modules.Sessions.Infrastructure.Persistence;
using TheLeague.Shared.Contracts.Events;
using TheLeague.Shared.Contracts.Messaging;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Enums;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Sessions.Application.Commands;

public record CancelBookingCommand(
    Guid SessionId,
    Guid BookingId
) : IRequest<Result>;

public class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand, Result>
{
    private readonly SessionsDbContext _db;
    private readonly ITenantService _tenantService;
    private readonly IIntegrationEventBus _eventBus;

    public CancelBookingCommandHandler(SessionsDbContext db, ITenantService tenantService, IIntegrationEventBus eventBus)
    {
        _db = db;
        _tenantService = tenantService;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
    {
        var clubId = _tenantService.CurrentTenantId;
        if (clubId is null)
            return Result.Failure("Tenant context is required.");

        var booking = await _db.SessionBookings
            .Include(b => b.Session)
            .FirstOrDefaultAsync(b => b.Id == request.BookingId && b.SessionId == request.SessionId, cancellationToken);

        if (booking is null)
            return Result.Failure("Booking not found.");

        if (booking.Status != BookingStatus.Confirmed)
            return Result.Failure("Only confirmed bookings can be cancelled.");

        var session = booking.Session!;

        // Enforce cancellation deadline
        if (!session.CanCancelBooking(DateTime.UtcNow))
            return Result.Failure($"Cancellation deadline has passed. Bookings must be cancelled at least {session.CancellationDeadlineHours} hours before the session start time.");

        booking.Cancel();
        session.DecrementBookings();

        await _db.SaveChangesAsync(cancellationToken);

        await _eventBus.PublishAsync(
            new BookingCancelledEvent(booking.Id, session.Id, booking.MemberId, clubId.Value),
            cancellationToken);

        // Promote from waitlist
        var nextWaitlist = await _db.Waitlists
            .Where(w => w.SessionId == request.SessionId && w.Status == WaitlistStatus.Waiting)
            .OrderBy(w => w.Position)
            .FirstOrDefaultAsync(cancellationToken);

        if (nextWaitlist is not null)
        {
            nextWaitlist.Offer();
            await _db.SaveChangesAsync(cancellationToken);
        }

        return Result.Success("Booking cancelled successfully.");
    }
}
