using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Facilities.Domain;
using TheLeague.Modules.Facilities.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Facilities.Application.Commands;

public record CancelFacilityBookingCommand(Guid BookingId) : IRequest<Result>;

public class CancelFacilityBookingCommandHandler : IRequestHandler<CancelFacilityBookingCommand, Result>
{
    private readonly FacilitiesDbContext _db;

    public CancelFacilityBookingCommandHandler(FacilitiesDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(CancelFacilityBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await _db.FacilityBookings
            .FirstOrDefaultAsync(b => b.Id == request.BookingId, cancellationToken);

        if (booking == null)
            return Result.Failure("Booking not found.");

        if (booking.Status == BookingStatus.Cancelled)
            return Result.Failure("Booking is already cancelled.");

        booking.Cancel();
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success("Booking cancelled successfully.");
    }
}
