using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Members.Domain;
using TheLeague.Modules.Members.Infrastructure.Persistence;
using TheLeague.Shared.Contracts.Events;
using TheLeague.Shared.Contracts.Messaging;
using TheLeague.Shared.Domain.Enums;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Members.Application.Commands;

public record ChangeMemberStatusCommand(
    Guid MemberId,
    MemberStatus NewStatus,
    string? ChangedByUserId
) : IRequest<Result>;

public class ChangeMemberStatusCommandHandler : IRequestHandler<ChangeMemberStatusCommand, Result>
{
    private readonly MembersDbContext _db;
    private readonly IIntegrationEventBus _eventBus;

    public ChangeMemberStatusCommandHandler(MembersDbContext db, IIntegrationEventBus eventBus)
    {
        _db = db;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(ChangeMemberStatusCommand request, CancellationToken cancellationToken)
    {
        var member = await _db.Members.FirstOrDefaultAsync(m => m.Id == request.MemberId, cancellationToken);

        if (member is null)
            return Result.Failure("Member not found.");

        var previousStatus = member.Status;

        try
        {
            switch (request.NewStatus)
            {
                case MemberStatus.Active:
                    if (member.Status == MemberStatus.Suspended || member.Status == MemberStatus.Expired)
                        member.Reactivate();
                    else
                        member.Activate();
                    break;
                case MemberStatus.Suspended:
                    member.Suspend();
                    break;
                case MemberStatus.Cancelled:
                    member.Cancel();
                    break;
                case MemberStatus.Expired:
                    member.Expire();
                    break;
                default:
                    return Result.Failure($"Cannot transition to status {request.NewStatus}.");
            }
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }

        var transition = MemberStatusTransition.Create(
            member.Id, member.ClubId, previousStatus, request.NewStatus, request.ChangedByUserId);

        _db.MemberStatusTransitions.Add(transition);
        await _db.SaveChangesAsync(cancellationToken);

        await _eventBus.PublishAsync(
            new MemberStatusChangedEvent(member.Id, member.ClubId, previousStatus, request.NewStatus),
            cancellationToken);

        return Result.Success($"Member status changed from {previousStatus} to {request.NewStatus}.");
    }
}
