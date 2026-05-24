using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Members.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Members.Application.Commands;

public record RemoveFamilyMemberCommand(Guid FamilyMemberId) : IRequest<Result>;

public class RemoveFamilyMemberCommandHandler : IRequestHandler<RemoveFamilyMemberCommand, Result>
{
    private readonly MembersDbContext _db;

    public RemoveFamilyMemberCommandHandler(MembersDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(RemoveFamilyMemberCommand request, CancellationToken cancellationToken)
    {
        var familyMember = await _db.FamilyMembers
            .FirstOrDefaultAsync(f => f.Id == request.FamilyMemberId, cancellationToken);

        if (familyMember is null)
            return Result.Failure("Family member link not found.");

        _db.FamilyMembers.Remove(familyMember);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success("Family member link removed.");
    }
}
