using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Memberships.Application.Dtos;
using TheLeague.Modules.Memberships.Domain;
using TheLeague.Modules.Memberships.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Memberships.Application.Commands;

public record PromoteFromWaitlistCommand(Guid MembershipTypeId) : IRequest<Result<MembershipWaitlistDto>>;

public class PromoteFromWaitlistCommandHandler : IRequestHandler<PromoteFromWaitlistCommand, Result<MembershipWaitlistDto>>
{
    private readonly MembershipsDbContext _db;

    public PromoteFromWaitlistCommandHandler(MembershipsDbContext db)
    {
        _db = db;
    }

    public async Task<Result<MembershipWaitlistDto>> Handle(PromoteFromWaitlistCommand request, CancellationToken cancellationToken)
    {
        var nextInQueue = await _db.MembershipWaitlists
            .Where(x => x.MembershipTypeId == request.MembershipTypeId
                && x.Status == WaitlistStatus.Waiting)
            .OrderBy(x => x.Position)
            .FirstOrDefaultAsync(cancellationToken);

        if (nextInQueue is null)
            return Result.Failure<MembershipWaitlistDto>("No members on the waitlist.");

        nextInQueue.MarkNotified();
        await _db.SaveChangesAsync(cancellationToken);

        var dto = new MembershipWaitlistDto(
            nextInQueue.Id, nextInQueue.MembershipTypeId, nextInQueue.MemberId,
            nextInQueue.Position, nextInQueue.RequestedAt, nextInQueue.NotifiedAt,
            nextInQueue.Status);

        return Result.Success(dto);
    }
}
