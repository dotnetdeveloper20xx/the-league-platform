using TheLeague.Shared.Domain.Enums;

namespace TheLeague.Modules.Programs.Application.Dtos;

public record ProgramDto(
    Guid Id,
    string Name,
    string? Description,
    ProgramType ProgramType,
    SkillLevel SkillLevel,
    int Capacity,
    decimal Price,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive,
    DateTime CreatedAt);

public record ProgramDetailDto(
    Guid Id,
    string Name,
    string? Description,
    ProgramType ProgramType,
    SkillLevel SkillLevel,
    int Capacity,
    decimal Price,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive,
    DateTime CreatedAt,
    List<ProgramSessionDto> Sessions,
    List<ProgramEnrollmentDto> Enrollments);

public record ProgramSessionDto(
    Guid Id,
    Guid ProgramId,
    string Title,
    Guid? InstructorId,
    string? InstructorName,
    DateTime StartDateTime,
    DateTime EndDateTime,
    Guid? VenueId,
    string? VenueName,
    int MaxCapacity,
    int SessionOrder);

public record ProgramEnrollmentDto(
    Guid Id,
    Guid ProgramId,
    Guid MemberId,
    EnrollmentStatus Status,
    DateTime EnrolledAt,
    DateTime? CompletedAt,
    int? WaitlistPosition);

public record ProgramAttendanceDto(
    Guid Id,
    Guid ProgramSessionId,
    Guid MemberId,
    bool IsPresent,
    DateTime MarkedAt);

public record MemberCertificateDto(
    Guid Id,
    Guid MemberId,
    Guid ProgramId,
    string ProgramName,
    SkillLevel SkillLevel,
    DateTime CompletionDate,
    string CertificateNumber);
