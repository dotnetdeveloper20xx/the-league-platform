using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Competitions.Application.Dtos;
using TheLeague.Modules.Competitions.Infrastructure.Persistence;

namespace TheLeague.Modules.Competitions.Application.Queries;

public record GetStandingsQuery(Guid CompetitionId) : IRequest<List<CompetitionStandingDto>>;

public class GetStandingsQueryHandler : IRequestHandler<GetStandingsQuery, List<CompetitionStandingDto>>
{
    private readonly CompetitionsDbContext _db;

    public GetStandingsQueryHandler(CompetitionsDbContext db)
    {
        _db = db;
    }

    public async Task<List<CompetitionStandingDto>> Handle(GetStandingsQuery request, CancellationToken cancellationToken)
    {
        return await _db.CompetitionStandings
            .Where(s => s.CompetitionId == request.CompetitionId)
            .OrderByDescending(s => s.Points)
            .ThenByDescending(s => s.GoalDifference)
            .ThenByDescending(s => s.GoalsFor)
            .Select(s => new CompetitionStandingDto(
                s.Id, s.CompetitionId, s.TeamId,
                s.Played, s.Won, s.Drawn, s.Lost,
                s.GoalsFor, s.GoalsAgainst, s.GoalDifference,
                s.Points, s.Form))
            .ToListAsync(cancellationToken);
    }
}
