using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Programs.Application.Dtos;
using TheLeague.Modules.Programs.Domain;
using TheLeague.Modules.Programs.Infrastructure.Persistence;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Programs.Application.Commands;

public record CreateProgramSessionCommand(
    Guid ProgramId,
    string Title,
    Guid? InstructorId,
    string? InstructorName,
    DateTime StartDateTime,
    DateTime EndDateTime,
    Guid? VenueId,
    string? VenueName,
    int MaxCapacity,
    int SessionOrder
) : IRequest<Result<ProgramSessionDto>>;

public class CreateProgramSessionCommandHandler : IRequestHandler<CreateProgramSessionCommand, Result<ProgramSessionDto>>
{
    private readonly ProgramsDbContext _db;
    private readonly ITenantService _tenantService;

    public CreateProgramSessionCommandHandler(ProgramsDbContext db, ITenantService tenantService)
    {
        _db = db;
        _tenantService = tenantService;
    }

    public async Task<Result<ProgramSessionDto>> Handle(CreateProgramSessionCommand request, CancellationToken cancellationToken)
    {
        var clubId = _tenantService.CurrentTenantId;
        if (clubId is null)
            return Result.Failure<ProgramSessionDto>("Tenant context is required.");

        var program = await _db.Programs.FirstOrDefaultAsync(p => p.Id == request.ProgramId, cancellationToken);
        if (program is null)
            return Result.Failure<ProgramSessionDto>("Program not found.");

        var session = ProgramSession.Create(
            clubId.Value,
            request.ProgramId,
            request.Title,
            request.InstructorId,
            request.InstructorName,
            request.StartDateTime,
            request.EndDateTime,
            request.VenueId,
            request.VenueName,
            request.MaxCapacity,
            request.SessionOrder);

        _db.ProgramSessions.Add(session);
        await _db.SaveChangesAsync(cancellationToken);

        var dto = new ProgramSessionDto(
            session.Id, session.ProgramId, session.Title,
            session.InstructorId, session.InstructorName,
            session.StartDateTime, session.EndDateTime,
            session.VenueId, session.VenueName,
            session.MaxCapacity, session.SessionOrder);

        return Result.Success(dto);
    }
}
