using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Sessions.Application.Dtos;
using TheLeague.Modules.Sessions.Domain;
using TheLeague.Modules.Sessions.Infrastructure.Persistence;
using TheLeague.Shared.Contracts.Events;
using TheLeague.Shared.Contracts.Messaging;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Sessions.Application.Commands;

public record BookSessionCommand(
    Guid SessionId,
    Guid MemberId
) : IRequest<Result<BookSessionResult>>;

public record BookSessionResult(
    Guid? BookingId,
    Guid? WaitlistId,
    int? WaitlistPosition,
    bool IsWaitlisted);

public class BookSessionCommandHandler : IRequestHandler<BookSessionCommand, Result<BookSessionResult>>
{
    private readonly SessionsDbContext _db;
    private readonly ITenantService _tenantService;
    private readonly IIntegrationEventBus _eventBus;

    public BookSessionCommandHandler(SessionsDbContext db, ITenantService tenantService, IIntegrationEventBus eventBus)
    {
        _db = db;
        _tenantService = tenantService;
        _eventBus = eventBus;
    }

    public async Task<Result<BookSessionResult>> Handle(BookSessionCommand request, CancellationToken cancellationToken)
    {
        var clubId = _tenantService.CurrentTenantId;
        if (clubId is null)
            return Result.Failure<BookSessionResult>("Tenant context is required.");

        var session = await _db.Sessions.FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);
        if (session is null)
            return Result.Failure<BookSessionResult>("Session not found.");

        if (session.IsCancelled)
            return Result.Failure<BookSessionResult>("Cannot book a cancelled session.");

        // Check if member already has a booking
        var existingBooking = await _db.SessionBookings
            .AnyAsync(b => b.SessionId == request.SessionId && b.MemberId == request.MemberId
                && b.Status == Shared.Domain.Enums.BookingStatus.Confirmed, cancellationToken);

        if (existingBooking)
            return Result.Failure<BookSessionResult>("Member already has a confirmed booking for this session.");

        // Check if capacity available
        if (session.CanBook())
        {
            var booking = SessionBooking.Create(clubId.Value, request.SessionId, request.MemberId);
            session.IncrementBookings();

            _db.SessionBookings.Add(booking);
            await _db.SaveChangesAsync(cancellationToken);

            await _eventBus.PublishAsync(
                new BookingConfirmedEvent(booking.Id, session.Id, request.MemberId, clubId.Value),
                cancellationToken);

            return Result.Success(new BookSessionResult(booking.Id, null, null, false));
        }

        // Waitlist logic
        var waitlistCount = await _db.Waitlists
            .CountAsync(w => w.SessionId == request.SessionId
                && (w.Status == WaitlistStatus.Waiting || w.Status == WaitlistStatus.Offered),
                cancellationToken);

        if (waitlistCount >= 50)
            return Result.Failure<BookSessionResult>("Session is full and waitlist is at maximum capacity (50).");

        // Check if already on waitlist
        var existingWaitlist = await _db.Waitlists
            .AnyAsync(w => w.SessionId == request.SessionId && w.MemberId == request.MemberId
                && (w.Status == WaitlistStatus.Waiting || w.Status == WaitlistStatus.Offered),
                cancellationToken);

        if (existingWaitlist)
            return Result.Failure<BookSessionResult>("Member is already on the waitlist for this session.");

        var position = waitlistCount + 1;
        var waitlistEntry = Waitlist.Create(clubId.Value, request.SessionId, request.MemberId, position);

        _db.Waitlists.Add(waitlistEntry);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(new BookSessionResult(null, waitlistEntry.Id, position, true));
    }
}
