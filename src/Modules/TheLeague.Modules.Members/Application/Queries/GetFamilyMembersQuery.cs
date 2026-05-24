using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Members.Application.Dtos;
using TheLeague.Modules.Members.Infrastructure.Persistence;

namespace TheLeague.Modules.Members.Application.Queries;

public record GetFamilyMembersQuery(Guid PrimaryMemberId) : IRequest<List<FamilyMemberDto>>;

public class GetFamilyMembersQueryHandler : IRequestHandler<GetFamilyMembersQuery, List<FamilyMemberDto>>
{
    private readonly MembersDbContext _db;

    public GetFamilyMembersQueryHandler(MembersDbContext db)
    {
        _db = db;
    }

    public async Task<List<FamilyMemberDto>> Handle(GetFamilyMembersQuery request, CancellationToken cancellationToken)
    {
        var familyMembers = await _db.FamilyMembers
            .AsNoTracking()
            .Where(f => f.PrimaryMemberId == request.PrimaryMemberId)
            .ToListAsync(cancellationToken);

        var dependentIds = familyMembers.Select(f => f.DependentMemberId).ToList();
        var dependents = await _db.Members
            .AsNoTracking()
            .Where(m => dependentIds.Contains(m.Id))
            .ToDictionaryAsync(m => m.Id, cancellationToken);

        return familyMembers.Select(f =>
        {
            var dep = dependents.GetValueOrDefault(f.DependentMemberId);
            return new FamilyMemberDto(
                f.Id,
                f.PrimaryMemberId,
                f.DependentMemberId,
                dep?.FirstName ?? "",
                dep?.LastName ?? "",
                dep?.Email ?? "",
                f.Relationship
            );
        }).ToList();
    }
}
