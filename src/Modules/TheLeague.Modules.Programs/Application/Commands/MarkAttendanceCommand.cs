using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Programs.Application.Dtos;
using TheLeague.Modules.Programs.Domain;
using TheLeague.Modules.Programs.Infrastructure.Persistence;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Programs.Application.Commands;

public record MarkProgramAttendanceCommand(
    Guid ProgramId,
    Guid SessionId,
    List<AttendanceEntry> Entries
) : IRequest<Result<List<ProgramAttendanceDto>>>;

public record AttendanceEntry(Guid MemberId, bool IsPresent);

public class MarkProgramAttendanceCommandHandler : IRequestHandler<MarkProgramAttendanceCommand, Result<List<ProgramAttendanceDto>>>
{
    private readonly ProgramsDbContext _db;
    private readonly ITenantService _tenantService;

    public MarkProgramAttendanceCommandHandler(ProgramsDbContext db, ITenantService tenantService)
    {
        _db = db;
        _tenantService = tenantService;
    }

    public async Task<Result<List<ProgramAttendanceDto>>> Handle(MarkProgramAttendanceCommand request, CancellationToken cancellationToken)
    {
        var clubId = _tenantService.CurrentTenantId;
        if (clubId is null)
            return Result.Failure<List<ProgramAttendanceDto>>("Tenant context is required.");

        var session = await _db.ProgramSessions
            .FirstOrDefaultAsync(s => s.Id == request.SessionId && s.ProgramId == request.ProgramId, cancellationToken);

        if (session is null)
            return Result.Failure<List<ProgramAttendanceDto>>("Program session not found.");

        var results = new List<ProgramAttendanceDto>();

        foreach (var entry in request.Entries)
        {
            var existing = await _db.ProgramAttendances
                .FirstOrDefaultAsync(a => a.ProgramSessionId == request.SessionId && a.MemberId == entry.MemberId,
                    cancellationToken);

            if (existing is not null)
            {
                existing.UpdatePresence(entry.IsPresent);
                results.Add(new ProgramAttendanceDto(
                    existing.Id, existing.ProgramSessionId, existing.MemberId,
                    existing.IsPresent, existing.MarkedAt));
            }
            else
            {
                var attendance = ProgramAttendance.Create(clubId.Value, request.SessionId, entry.MemberId, entry.IsPresent);
                _db.ProgramAttendances.Add(attendance);
                results.Add(new ProgramAttendanceDto(
                    attendance.Id, attendance.ProgramSessionId, attendance.MemberId,
                    attendance.IsPresent, attendance.MarkedAt));
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success(results);
    }
}
