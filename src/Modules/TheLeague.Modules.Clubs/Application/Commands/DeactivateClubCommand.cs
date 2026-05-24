using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Clubs.Infrastructure.Persistence;
using TheLeague.Shared.Contracts.Events;
using TheLeague.Shared.Contracts.Messaging;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Clubs.Application.Commands;

public record DeactivateClubCommand(Guid Id) : IRequest<Result>;

public class DeactivateClubCommandHandler : IRequestHandler<DeactivateClubCommand, Result>
{
    private readonly ClubsDbContext _db;
    private readonly IIntegrationEventBus _eventBus;

    public DeactivateClubCommandHandler(ClubsDbContext db, IIntegrationEventBus eventBus)
    {
        _db = db;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(DeactivateClubCommand request, CancellationToken cancellationToken)
    {
        var club = await _db.Clubs.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (club is null)
            return Result.Failure("Club not found.");

        club.Deactivate();

        await _db.SaveChangesAsync(cancellationToken);

        await _eventBus.PublishAsync(new ClubDeactivatedEvent(club.Id), cancellationToken);

        return Result.Success("Club deactivated successfully.");
    }
}
