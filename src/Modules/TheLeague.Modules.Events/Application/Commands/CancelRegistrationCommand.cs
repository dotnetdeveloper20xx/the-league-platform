using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Events.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Events.Application.Commands;

public record CancelRegistrationCommand(Guid EventId, Guid RegistrationId) : IRequest<Result>;

public class CancelRegistrationCommandHandler : IRequestHandler<CancelRegistrationCommand, Result>
{
    private readonly EventsDbContext _db;

    public CancelRegistrationCommandHandler(EventsDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(CancelRegistrationCommand request, CancellationToken cancellationToken)
    {
        var registration = await _db.EventRegistrations
            .FirstOrDefaultAsync(r => r.Id == request.RegistrationId && r.EventId == request.EventId, cancellationToken);

        if (registration is null)
            return Result.Failure("Registration not found.");

        if (registration.CancelledAt is not null)
            return Result.Failure("Registration is already cancelled.");

        var evt = await _db.Events.FirstOrDefaultAsync(e => e.Id == request.EventId, cancellationToken);
        if (evt is null)
            return Result.Failure("Event not found.");

        // Enforce 48-hour cancellation deadline
        var hoursUntilEvent = (evt.StartDateTime - DateTime.UtcNow).TotalHours;
        if (hoursUntilEvent < evt.CancellationDeadlineHours)
            return Result.Failure($"Cannot cancel registration within {evt.CancellationDeadlineHours} hours of event start time.");

        registration.Cancel();
        evt.DecrementRegistrationCount();

        // If ticketed, initiate refund
        if (registration.RegistrationType == "Ticket")
        {
            registration.InitiateRefund();
        }

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success("Registration cancelled successfully.");
    }
}
