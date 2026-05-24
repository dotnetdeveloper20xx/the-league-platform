using TheLeague.Shared.Domain.Entities;
using TheLeague.Shared.Domain.Enums;

namespace TheLeague.Modules.Members.Domain;

public class MemberStatusTransition : BaseEntity
{
    public Guid MemberId { get; private set; }
    public Guid ClubId { get; private set; }
    public MemberStatus PreviousStatus { get; private set; }
    public MemberStatus NewStatus { get; private set; }
    public DateTime ChangedAt { get; private set; }
    public string? ChangedByUserId { get; private set; }

    private MemberStatusTransition() { }

    public static MemberStatusTransition Create(
        Guid memberId,
        Guid clubId,
        MemberStatus previousStatus,
        MemberStatus newStatus,
        string? changedByUserId)
    {
        return new MemberStatusTransition
        {
            MemberId = memberId,
            ClubId = clubId,
            PreviousStatus = previousStatus,
            NewStatus = newStatus,
            ChangedAt = DateTime.UtcNow,
            ChangedByUserId = changedByUserId
        };
    }
}
