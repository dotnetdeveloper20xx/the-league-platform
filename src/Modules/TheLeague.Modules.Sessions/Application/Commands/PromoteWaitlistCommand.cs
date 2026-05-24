using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Sessions.Domain;
using TheLeague.Modules.Sessions.Infrastructure.Persistence;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Sessions.Application.Commands;

public record PromoteWaitlistCommand(
    Guid SessionId
) : IRequest<Result>;

public class PromoteWaitlistCommandHandler : IRequestHandler<PromoteWaitlistCommand, Result>
{
    private readonly SessionsDbContext _db;
    private readonly ITenantService _tenantService;

    public PromoteWaitlistCommandHandler(SessionsDbContext db, ITenantService tenantService)
    {
        _db = db;
        _tenantService = tenantService;
    }

    public async Task<Result> Handle(PromoteWaitlistCommand request, CancellationToken cancellationToken)
    {
        var clubId = _tenantService.CurrentTenantId;
        if (clubId is null)
            return Result.Failure("Tenant context is required.");

        var session = await _db.Sessions.FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);
        if (session is null)
            return Result.Failure("Session not found.");

        // Check for expired offers and expire them
        var expiredOffers = await _db.Waitlists
            .Where(w => w.SessionId == request.SessionId
                && w.Status == WaitlistStatus.Offered
                && w.ExpiresAt != null
                && w.ExpiresAt < DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        foreach (var expired in expiredOffers)
        {
            expired.Expire();
        }

        // Find next waiting member
        var nextWaiting = await _db.Waitlists
            .Where(w => w.SessionId == request.SessionId && w.Status == WaitlistStatus.Waiting)
            .OrderBy(w => w.Position)
            .FirstOrDefaultAsync(cancellationToken);

        if (nextWaiting is null)
            return Result.Success("No members on waitlist to promote.");

        // Offer the slot (24-hour window)
        nextWaiting.Offer();
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success($"Slot offered to waitlist member at position {nextWaiting.Position}. Offer expires in 24 hours.");
    }
}
