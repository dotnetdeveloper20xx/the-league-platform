using MediatR;
using TheLeague.Modules.Competitions.Application.Dtos;
using TheLeague.Modules.Competitions.Domain;
using TheLeague.Modules.Competitions.Infrastructure.Persistence;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Competitions.Application.Commands;

public record CreateSeasonCommand(
    string Name,
    DateTime StartDate,
    DateTime EndDate
) : IRequest<Result<SeasonDto>>;

public class CreateSeasonCommandHandler : IRequestHandler<CreateSeasonCommand, Result<SeasonDto>>
{
    private readonly CompetitionsDbContext _db;
    private readonly ITenantService _tenantService;

    public CreateSeasonCommandHandler(CompetitionsDbContext db, ITenantService tenantService)
    {
        _db = db;
        _tenantService = tenantService;
    }

    public async Task<Result<SeasonDto>> Handle(CreateSeasonCommand request, CancellationToken cancellationToken)
    {
        var clubId = _tenantService.CurrentTenantId!.Value;

        var season = Season.Create(clubId, request.Name, request.StartDate, request.EndDate);

        _db.Seasons.Add(season);
        await _db.SaveChangesAsync(cancellationToken);

        var dto = new SeasonDto(
            season.Id, season.ClubId, season.Name,
            season.StartDate, season.EndDate, season.IsActive, season.CreatedAt);

        return Result.Success(dto);
    }
}
