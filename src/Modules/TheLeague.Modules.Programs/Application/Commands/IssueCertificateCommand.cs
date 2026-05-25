using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Programs.Application.Dtos;
using TheLeague.Modules.Programs.Domain;
using TheLeague.Modules.Programs.Infrastructure.Persistence;
using TheLeague.Shared.Contracts.Events;
using TheLeague.Shared.Contracts.Messaging;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Enums;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Programs.Application.Commands;

public record IssueCertificateCommand(
    Guid ProgramId,
    Guid MemberId
) : IRequest<Result<MemberCertificateDto>>;

public class IssueCertificateCommandHandler : IRequestHandler<IssueCertificateCommand, Result<MemberCertificateDto>>
{
    private readonly ProgramsDbContext _db;
    private readonly ITenantService _tenantService;
    private readonly IIntegrationEventBus _eventBus;

    public IssueCertificateCommandHandler(ProgramsDbContext db, ITenantService tenantService, IIntegrationEventBus eventBus)
    {
        _db = db;
        _tenantService = tenantService;
        _eventBus = eventBus;
    }

    public async Task<Result<MemberCertificateDto>> Handle(IssueCertificateCommand request, CancellationToken cancellationToken)
    {
        var clubId = _tenantService.CurrentTenantId;
        if (clubId is null)
            return Result.Failure<MemberCertificateDto>("Tenant context is required.");

        var program = await _db.Programs
            .Include(p => p.Sessions)
            .FirstOrDefaultAsync(p => p.Id == request.ProgramId, cancellationToken);

        if (program is null)
            return Result.Failure<MemberCertificateDto>("Program not found.");

        // Validate program has ended
        if (!program.HasEnded(DateTime.UtcNow))
            return Result.Failure<MemberCertificateDto>("Cannot issue certificate before program end date.");

        // Check enrollment
        var enrollment = await _db.ProgramEnrollments
            .FirstOrDefaultAsync(e => e.ProgramId == request.ProgramId && e.MemberId == request.MemberId
                && (e.Status == EnrollmentStatus.Confirmed || e.Status == EnrollmentStatus.Completed),
                cancellationToken);

        if (enrollment is null)
            return Result.Failure<MemberCertificateDto>("Member is not enrolled in this program.");

        // Check if certificate already issued
        var existingCert = await _db.MemberCertificates
            .AnyAsync(c => c.ProgramId == request.ProgramId && c.MemberId == request.MemberId, cancellationToken);

        if (existingCert)
            return Result.Failure<MemberCertificateDto>("Certificate has already been issued for this member and program.");

        // Calculate attendance rate (≥80% required)
        var totalSessions = program.Sessions.Count;
        if (totalSessions == 0)
            return Result.Failure<MemberCertificateDto>("Program has no sessions. Cannot calculate attendance.");

        var attendedSessions = await _db.ProgramAttendances
            .CountAsync(a => a.Session.ProgramId == request.ProgramId
                && a.MemberId == request.MemberId
                && a.IsPresent,
                cancellationToken);

        var attendanceRate = (double)attendedSessions / totalSessions;
        if (attendanceRate < 0.8)
            return Result.Failure<MemberCertificateDto>(
                $"Attendance rate is {attendanceRate:P0} which is below the required 80%. Attended {attendedSessions} of {totalSessions} sessions.");

        // Generate certificate number
        var certificateNumber = $"CERT-{clubId.Value.ToString("N")[..8].ToUpper()}-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";

        var certificate = MemberCertificate.Create(
            clubId.Value,
            request.MemberId,
            request.ProgramId,
            program.Name,
            program.SkillLevel,
            DateTime.UtcNow,
            certificateNumber);

        _db.MemberCertificates.Add(certificate);

        // Mark enrollment as completed
        enrollment.Complete();

        await _db.SaveChangesAsync(cancellationToken);

        // Publish integration event
        await _eventBus.PublishAsync(
            new CertificateIssuedEvent(certificate.Id, request.MemberId, clubId.Value),
            cancellationToken);

        var dto = new MemberCertificateDto(
            certificate.Id, certificate.MemberId, certificate.ProgramId,
            certificate.ProgramName, certificate.SkillLevel,
            certificate.CompletionDate, certificate.CertificateNumber);

        return Result.Success(dto);
    }
}
