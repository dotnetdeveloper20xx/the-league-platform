using TheLeague.Shared.Domain.Enums;
using TheLeague.Shared.Domain.ValueObjects;

namespace TheLeague.Modules.Members.Application.Dtos;

public record MemberDto(
    Guid Id,
    Guid ClubId,
    Guid? UserId,
    string MemberNumber,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    DateTime? DateOfBirth,
    Gender? Gender,
    Address? Address,
    EmergencyContact? PrimaryEmergencyContact,
    EmergencyContact? SecondaryEmergencyContact,
    MedicalInfo? MedicalInfo,
    string? ProfilePhotoUrl,
    string? FacebookUrl,
    string? TwitterHandle,
    string? InstagramHandle,
    string? LinkedInUrl,
    string? CustomFieldValues,
    bool MarketingOptIn,
    bool SmsOptIn,
    bool EmailOptIn,
    bool IsFamilyAccount,
    Guid? PrimaryMemberId,
    MemberStatus Status,
    DateTime JoinedDate,
    DateTime? LastLoginDate,
    bool IsActive,
    string? QRCodeData,
    Guid? ReferredByMemberId,
    string? ReferralSource,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record MemberListDto(
    Guid Id,
    string MemberNumber,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    MemberStatus Status,
    DateTime JoinedDate,
    bool IsActive
);

public record FamilyMemberDto(
    Guid Id,
    Guid PrimaryMemberId,
    Guid DependentMemberId,
    string DependentFirstName,
    string DependentLastName,
    string DependentEmail,
    FamilyMemberRelation Relationship
);

public record CustomFieldDefinitionDto(
    Guid Id,
    string Name,
    string FieldType,
    bool IsRequired,
    string? Options,
    int DisplayOrder
);

public record ImportResultDto(
    int TotalRows,
    int ImportedCount,
    int RejectedCount,
    List<ImportErrorDto> Errors
);

public record ImportErrorDto(
    int RowNumber,
    string Field,
    string Reason
);
