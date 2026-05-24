using TheLeague.Shared.Domain.Entities;
using TheLeague.Shared.Domain.Enums;

namespace TheLeague.Modules.Members.Domain;

public class FamilyMember : TenantEntity
{
    public Guid PrimaryMemberId { get; private set; }
    public Guid DependentMemberId { get; private set; }
    public FamilyMemberRelation Relationship { get; private set; }

    private FamilyMember() { }

    public static FamilyMember Create(Guid clubId, Guid primaryMemberId, Guid dependentMemberId, FamilyMemberRelation relationship)
    {
        return new FamilyMember
        {
            ClubId = clubId,
            PrimaryMemberId = primaryMemberId,
            DependentMemberId = dependentMemberId,
            Relationship = relationship
        };
    }
}
