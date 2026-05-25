using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Programs.Application.Dtos;
using TheLeague.Modules.Programs.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Enums;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Programs.Application.Commands;

public record UpdateProgramCommand(
    Guid ProgramId,
    string Name,
    string? Description,
    ProgramType ProgramType,
    SkillLevel SkillLevel,
    int Capacity,
    decimal Price,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive
) : IRequest<Result<ProgramDto>>;

public class UpdateProgramCommandHandler : IRequestHandler<UpdateProgramCommand, Result<ProgramDto>>
{
    private readonly ProgramsDbContext _db;

    public UpdateProgramCommandHandler(ProgramsDbContext db)
    {
        _db = db;
    }

    public async Task<Result<ProgramDto>> Handle(UpdateProgramCommand request, CancellationToken cancellationToken)
    {
        var program = await _db.Programs.FirstOrDefaultAsync(p => p.Id == request.ProgramId, cancellationToken);
        if (program is null)
            return Result.Failure<ProgramDto>("Program not found.");

        program.Update(
            request.Name,
            request.Description,
            request.ProgramType,
            request.SkillLevel,
            request.Capacity,
            request.Price,
            request.StartDate,
            request.EndDate,
            request.IsActive);

        await _db.SaveChangesAsync(cancellationToken);

        var dto = new ProgramDto(
            program.Id, program.Name, program.Description,
            program.ProgramType, program.SkillLevel,
            program.Capacity, program.Price,
            program.StartDate, program.EndDate,
            program.IsActive, program.CreatedAt);

        return Result.Success(dto);
    }
}
