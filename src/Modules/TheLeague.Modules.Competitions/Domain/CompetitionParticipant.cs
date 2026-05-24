using TheLeague.Shared.Domain.Entities;

namespace TheLeague.Modules.Competitions.Domain;

public class CompetitionParticipant : TenantEntity
{
    public Guid TeamId { get; private set; }
    public Guid MemberId { get; private set; }
    public int? JerseyNumber { get; private set; }
    public bool IsActive { get; private set; } = true;

    private CompetitionParticipant() { }

    public static CompetitionParticipant Create(
        Guid clubId,
        Guid teamId,
        Guid memberId,
        int? jerseyNumber = null)
    {
        return new CompetitionParticipant
        {
            ClubId = clubId,
            TeamId = teamId,
            MemberId = memberId,
            JerseyNumber = jerseyNumber,
            IsActive = true
        };
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
