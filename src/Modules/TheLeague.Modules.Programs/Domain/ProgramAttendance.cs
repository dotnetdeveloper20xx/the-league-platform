using TheLeague.Shared.Domain.Entities;

namespace TheLeague.Modules.Programs.Domain;

public class ProgramAttendance : TenantEntity
{
    public Guid ProgramSessionId { get; private set; }
    public Guid MemberId { get; private set; }
    public bool IsPresent { get; private set; }
    public DateTime MarkedAt { get; private set; }

    // Navigation property
    public ProgramSession Session { get; private set; } = null!;

    public static ProgramAttendance Create(
        Guid clubId,
        Guid programSessionId,
        Guid memberId,
        bool isPresent)
    {
        return new ProgramAttendance
        {
            ClubId = clubId,
            ProgramSessionId = programSessionId,
            MemberId = memberId,
            IsPresent = isPresent,
            MarkedAt = DateTime.UtcNow
        };
    }

    public void UpdatePresence(bool isPresent)
    {
        IsPresent = isPresent;
        MarkedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
