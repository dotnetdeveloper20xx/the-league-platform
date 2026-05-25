using TheLeague.Shared.Domain.Entities;
using TheLeague.Shared.Domain.Enums;

namespace TheLeague.Modules.Programs.Domain;

public class MemberCertificate : TenantEntity
{
    public Guid MemberId { get; private set; }
    public Guid ProgramId { get; private set; }
    public string ProgramName { get; private set; } = string.Empty;
    public SkillLevel SkillLevel { get; private set; }
    public DateTime CompletionDate { get; private set; }
    public string CertificateNumber { get; private set; } = string.Empty;

    public static MemberCertificate Create(
        Guid clubId,
        Guid memberId,
        Guid programId,
        string programName,
        SkillLevel skillLevel,
        DateTime completionDate,
        string certificateNumber)
    {
        if (string.IsNullOrWhiteSpace(certificateNumber))
            throw new ArgumentException("Certificate number is required.");

        return new MemberCertificate
        {
            ClubId = clubId,
            MemberId = memberId,
            ProgramId = programId,
            ProgramName = programName,
            SkillLevel = skillLevel,
            CompletionDate = completionDate,
            CertificateNumber = certificateNumber
        };
    }
}
