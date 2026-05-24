using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Sessions.Infrastructure.Persistence;
using TheLeague.Shared.Contracts.Events;
using TheLeague.Shared.Contracts.Messaging;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Sessions.Application.Commands;

public record CancelSessionCommand(
    Guid SessionId,
    string Reason
) : IRequest<Result>;

public class CancelSessionCommandHandler : IRequestHandler<CancelSessionCommand, Result>
{
    private readonly SessionsDbContext _db;
    private readonly ITenantService _tenantService;
    private readonly IIntegrationEventBus _eventBus;

    public CancelSessionCommandHandler(SessionsDbContext db, ITenantService tenantService, IIntegrationEventBus eventBus)
    {
        _db = db;
        _tenantService = tenantService;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(CancelSessionCommand request, CancellationToken cancellationToken)
    {
        var clubId = _tenantService.CurrentTenantId;
        if (clubId is null)
            return Result.Failure("Tenant context is required.");

        var session = await _db.Sessions.FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);
        if (session is null)
            return Result.Failure("Session not found.");

        if (session.IsCancelled)
            return Result.Failure("Session is already cancelled.");

        session.Cancel(request.Reason);
        await _db.SaveChangesAsync(cancellationToken);

        await _eventBus.PublishAsync(
            new SessionCancelledEvent(session.Id, clubId.Value, request.Reason),
            cancellationToken);

        return Result.Success("Session cancelled successfully.");
    }
}
