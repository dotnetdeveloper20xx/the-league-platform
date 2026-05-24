using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Competitions.Application.Dtos;
using TheLeague.Modules.Competitions.Infrastructure.Persistence;

namespace TheLeague.Modules.Competitions.Application.Queries;

public record GetCompetitionsQuery(Guid? SeasonId = null) : IRequest<List<CompetitionDto>>;

public class GetCompetitionsQueryHandler : IRequestHandler<GetCompetitionsQuery, List<CompetitionDto>>
{
    private readonly CompetitionsDbContext _db;

    public GetCompetitionsQueryHandler(CompetitionsDbContext db)
    {
        _db = db;
    }

    public async Task<List<CompetitionDto>> Handle(GetCompetitionsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Competitions.AsQueryable();

        if (request.SeasonId.HasValue)
            query = query.Where(c => c.SeasonId == request.SeasonId.Value);

        return await query
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CompetitionDto(
                c.Id, c.ClubId, c.SeasonId, c.Name,
                c.CompetitionType, c.Status,
                c.PointsForWin, c.PointsForDraw, c.PointsForLoss,
                c.DefaultWalkoverScore, c.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
