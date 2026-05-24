using TheLeague.Shared.Domain.Entities;

namespace TheLeague.Modules.Members.Domain;

public class MemberNote : TenantEntity
{
    public Guid MemberId { get; private set; }
    public string NoteType { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public string? CreatedByUserId { get; private set; }

    private MemberNote() { }

    public static MemberNote Create(Guid clubId, Guid memberId, string noteType, string content, string? createdByUserId)
    {
        return new MemberNote
        {
            ClubId = clubId,
            MemberId = memberId,
            NoteType = noteType,
            Content = content,
            CreatedByUserId = createdByUserId
        };
    }
}
