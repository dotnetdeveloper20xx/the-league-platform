using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Competitions.Application.Dtos;
using TheLeague.Modules.Competitions.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Competitions.Application.Queries;

public record GetCompetitionByIdQuery(Guid Id) : IRequest<Result<CompetitionDetailDto>>;

public class GetCompetitionByIdQueryHandler : IRequestHandler<GetCompetitionByIdQuery, Result<CompetitionDetailDto>>
{
    private readonly CompetitionsDbContext _db;

    public GetCompetitionByIdQueryHandler(CompetitionsDbContext db)
    {
        _db = db;
    }

    public async Task<Result<CompetitionDetailDto>> Handle(GetCompetitionByIdQuery request, CancellationToken cancellationToken)
    {
        var competition = await _db.Competitions
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (competition == null)
            return Result.Failure<CompetitionDetailDto>("Competition not found.");

        var teamCount = await _db.CompetitionTeams
            .CountAsync(t => t.CompetitionId == request.Id, cancellationToken);

        var matchCount = await _db.Matches
            .CountAsync(m => m.CompetitionId == request.Id, cancellationToken);

        var dto = new CompetitionDetailDto(
            competition.Id, competition.ClubId, competition.SeasonId,
            competition.Name, competition.CompetitionType, competition.Status,
            competition.PointsForWin, competition.PointsForDraw, competition.PointsForLoss,
            competition.DefaultWalkoverScore, teamCount, matchCount, competition.CreatedAt);

        return Result.Success(dto);
    }
}
