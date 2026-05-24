using MediatR;
using TheLeague.Modules.Competitions.Application.Dtos;
using TheLeague.Modules.Competitions.Domain;
using TheLeague.Modules.Competitions.Infrastructure.Persistence;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Enums;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Competitions.Application.Commands;

public record CreateCompetitionCommand(
    Guid SeasonId,
    string Name,
    CompetitionType CompetitionType,
    int PointsForWin = 3,
    int PointsForDraw = 1,
    int PointsForLoss = 0,
    string? DefaultWalkoverScore = null
) : IRequest<Result<CompetitionDto>>;

public class CreateCompetitionCommandHandler : IRequestHandler<CreateCompetitionCommand, Result<CompetitionDto>>
{
    private readonly CompetitionsDbContext _db;
    private readonly ITenantService _tenantService;

    public CreateCompetitionCommandHandler(CompetitionsDbContext db, ITenantService tenantService)
    {
        _db = db;
        _tenantService = tenantService;
    }

    public async Task<Result<CompetitionDto>> Handle(CreateCompetitionCommand request, CancellationToken cancellationToken)
    {
        var clubId = _tenantService.CurrentTenantId!.Value;

        var competition = Competition.Create(
            clubId,
            request.SeasonId,
            request.Name,
            request.CompetitionType,
            request.PointsForWin,
            request.PointsForDraw,
            request.PointsForLoss,
            request.DefaultWalkoverScore);

        _db.Competitions.Add(competition);
        await _db.SaveChangesAsync(cancellationToken);

        var dto = new CompetitionDto(
            competition.Id, competition.ClubId, competition.SeasonId,
            competition.Name, competition.CompetitionType, competition.Status,
            competition.PointsForWin, competition.PointsForDraw, competition.PointsForLoss,
            competition.DefaultWalkoverScore, competition.CreatedAt);

        return Result.Success(dto);
    }
}
