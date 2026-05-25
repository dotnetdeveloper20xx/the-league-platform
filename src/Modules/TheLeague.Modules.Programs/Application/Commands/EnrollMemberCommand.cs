using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Programs.Application.Dtos;
using TheLeague.Modules.Programs.Domain;
using TheLeague.Modules.Programs.Infrastructure.Persistence;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Enums;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Programs.Application.Commands;

public record EnrollMemberCommand(
    Guid ProgramId,
    Guid MemberId
) : IRequest<Result<ProgramEnrollmentDto>>;

public class EnrollMemberCommandHandler : IRequestHandler<EnrollMemberCommand, Result<ProgramEnrollmentDto>>
{
    private readonly ProgramsDbContext _db;
    private readonly ITenantService _tenantService;

    public EnrollMemberCommandHandler(ProgramsDbContext db, ITenantService tenantService)
    {
        _db = db;
        _tenantService = tenantService;
    }

    public async Task<Result<ProgramEnrollmentDto>> Handle(EnrollMemberCommand request, CancellationToken cancellationToken)
    {
        var clubId = _tenantService.CurrentTenantId;
        if (clubId is null)
            return Result.Failure<ProgramEnrollmentDto>("Tenant context is required.");

        var program = await _db.Programs.FirstOrDefaultAsync(p => p.Id == request.ProgramId, cancellationToken);
        if (program is null)
            return Result.Failure<ProgramEnrollmentDto>("Program not found.");

        if (!program.IsActive)
            return Result.Failure<ProgramEnrollmentDto>("Program is not active.");

        // Check if member is already enrolled
        var existingEnrollment = await _db.ProgramEnrollments
            .AnyAsync(e => e.ProgramId == request.ProgramId && e.MemberId == request.MemberId
                && (e.Status == EnrollmentStatus.Confirmed || e.Status == EnrollmentStatus.WaitListed),
                cancellationToken);

        if (existingEnrollment)
            return Result.Failure<ProgramEnrollmentDto>("Member is already enrolled or waitlisted for this program.");

        // Count confirmed enrollments
        var confirmedCount = await _db.ProgramEnrollments
            .CountAsync(e => e.ProgramId == request.ProgramId && e.Status == EnrollmentStatus.Confirmed,
                cancellationToken);

        if (confirmedCount < program.Capacity)
        {
            // Enroll directly
            var enrollment = ProgramEnrollment.Create(clubId.Value, request.ProgramId, request.MemberId, EnrollmentStatus.Confirmed);
            _db.ProgramEnrollments.Add(enrollment);
            await _db.SaveChangesAsync(cancellationToken);

            var dto = new ProgramEnrollmentDto(
                enrollment.Id, enrollment.ProgramId, enrollment.MemberId,
                enrollment.Status, enrollment.EnrolledAt, enrollment.CompletedAt,
                enrollment.WaitlistPosition);

            return Result.Success(dto);
        }

        // Waitlist logic
        var waitlistCount = await _db.ProgramEnrollments
            .CountAsync(e => e.ProgramId == request.ProgramId && e.Status == EnrollmentStatus.WaitListed,
                cancellationToken);

        if (waitlistCount >= 50)
            return Result.Failure<ProgramEnrollmentDto>("Program is full and waitlist is at maximum capacity (50).");

        var position = waitlistCount + 1;
        var waitlistEnrollment = ProgramEnrollment.Create(
            clubId.Value, request.ProgramId, request.MemberId, EnrollmentStatus.WaitListed, position);

        _db.ProgramEnrollments.Add(waitlistEnrollment);
        await _db.SaveChangesAsync(cancellationToken);

        var waitlistDto = new ProgramEnrollmentDto(
            waitlistEnrollment.Id, waitlistEnrollment.ProgramId, waitlistEnrollment.MemberId,
            waitlistEnrollment.Status, waitlistEnrollment.EnrolledAt, waitlistEnrollment.CompletedAt,
            waitlistEnrollment.WaitlistPosition);

        return Result.Success(waitlistDto);
    }
}
