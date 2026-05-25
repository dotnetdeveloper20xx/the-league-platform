using MediatR;
using TheLeague.Modules.Programs.Application.Dtos;
using TheLeague.Modules.Programs.Infrastructure.Persistence;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Enums;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Programs.Application.Commands;

public record CreateProgramCommand(
    string Name,
    string? Description,
    ProgramType ProgramType,
    SkillLevel SkillLevel,
    int Capacity,
    decimal Price,
    DateTime StartDate,
    DateTime EndDate
) : IRequest<Result<ProgramDto>>;

public class CreateProgramCommandHandler : IRequestHandler<CreateProgramCommand, Result<ProgramDto>>
{
    private readonly ProgramsDbContext _db;
    private readonly ITenantService _tenantService;

    public CreateProgramCommandHandler(ProgramsDbContext db, ITenantService tenantService)
    {
        _db = db;
        _tenantService = tenantService;
    }

    public async Task<Result<ProgramDto>> Handle(CreateProgramCommand request, CancellationToken cancellationToken)
    {
        var clubId = _tenantService.CurrentTenantId;
        if (clubId is null)
            return Result.Failure<ProgramDto>("Tenant context is required.");

        var program = Domain.Program.Create(
            clubId.Value,
            request.Name,
            request.Description,
            request.ProgramType,
            request.SkillLevel,
            request.Capacity,
            request.Price,
            request.StartDate,
            request.EndDate);

        _db.Programs.Add(program);
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
