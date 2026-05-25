using TheLeague.Shared.Domain.Entities;
using TheLeague.Shared.Domain.Enums;

namespace TheLeague.Modules.Programs.Domain;

public class ProgramEnrollment : TenantEntity
{
    public Guid ProgramId { get; private set; }
    public Guid MemberId { get; private set; }
    public EnrollmentStatus Status { get; private set; }
    public DateTime EnrolledAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public int? WaitlistPosition { get; private set; }

    // Navigation property
    public Program Program { get; private set; } = null!;

    public static ProgramEnrollment Create(
        Guid clubId,
        Guid programId,
        Guid memberId,
        EnrollmentStatus status,
        int? waitlistPosition = null)
    {
        return new ProgramEnrollment
        {
            ClubId = clubId,
            ProgramId = programId,
            MemberId = memberId,
            Status = status,
            EnrolledAt = DateTime.UtcNow,
            WaitlistPosition = waitlistPosition
        };
    }

    public void Confirm()
    {
        Status = EnrollmentStatus.Confirmed;
        WaitlistPosition = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Withdraw()
    {
        Status = EnrollmentStatus.Withdrawn;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        Status = EnrollmentStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
