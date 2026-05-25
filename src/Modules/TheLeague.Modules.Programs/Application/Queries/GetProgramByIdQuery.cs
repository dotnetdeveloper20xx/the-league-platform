using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Programs.Application.Dtos;
using TheLeague.Modules.Programs.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Programs.Application.Queries;

public record GetProgramByIdQuery(Guid ProgramId) : IRequest<Result<ProgramDetailDto>>;

public class GetProgramByIdQueryHandler : IRequestHandler<GetProgramByIdQuery, Result<ProgramDetailDto>>
{
    private readonly ProgramsDbContext _db;

    public GetProgramByIdQueryHandler(ProgramsDbContext db)
    {
        _db = db;
    }

    public async Task<Result<ProgramDetailDto>> Handle(GetProgramByIdQuery request, CancellationToken cancellationToken)
    {
        var program = await _db.Programs
            .AsNoTracking()
            .Include(p => p.Sessions.OrderBy(s => s.SessionOrder))
            .Include(p => p.Enrollments)
            .FirstOrDefaultAsync(p => p.Id == request.ProgramId, cancellationToken);

        if (program is null)
            return Result.Failure<ProgramDetailDto>("Program not found.");

        var sessions = program.Sessions.Select(s => new ProgramSessionDto(
            s.Id, s.ProgramId, s.Title,
            s.InstructorId, s.InstructorName,
            s.StartDateTime, s.EndDateTime,
            s.VenueId, s.VenueName,
            s.MaxCapacity, s.SessionOrder)).ToList();

        var enrollments = program.Enrollments.Select(e => new ProgramEnrollmentDto(
            e.Id, e.ProgramId, e.MemberId,
            e.Status, e.EnrolledAt, e.CompletedAt,
            e.WaitlistPosition)).ToList();

        var dto = new ProgramDetailDto(
            program.Id, program.Name, program.Description,
            program.ProgramType, program.SkillLevel,
            program.Capacity, program.Price,
            program.StartDate, program.EndDate,
            program.IsActive, program.CreatedAt,
            sessions, enrollments);

        return Result.Success(dto);
    }
}
