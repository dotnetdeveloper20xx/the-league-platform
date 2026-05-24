using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Competitions.Application.Dtos;
using TheLeague.Modules.Competitions.Infrastructure.Persistence;

namespace TheLeague.Modules.Competitions.Application.Queries;

public record GetFixturesQuery(Guid CompetitionId) : IRequest<List<MatchDto>>;

public class GetFixturesQueryHandler : IRequestHandler<GetFixturesQuery, List<MatchDto>>
{
    private readonly CompetitionsDbContext _db;

    public GetFixturesQueryHandler(CompetitionsDbContext db)
    {
        _db = db;
    }

    public async Task<List<MatchDto>> Handle(GetFixturesQuery request, CancellationToken cancellationToken)
    {
        return await _db.Matches
            .Where(m => m.CompetitionId == request.CompetitionId)
            .OrderBy(m => m.RoundNumber)
            .ThenBy(m => m.ScheduledDateTime)
            .Select(m => new MatchDto(
                m.Id, m.CompetitionId, m.RoundNumber,
                m.HomeTeamId, m.AwayTeamId, m.VenueId, m.VenueName,
                m.ScheduledDateTime, m.Status, m.HomeScore, m.AwayScore, m.Result))
            .ToListAsync(cancellationToken);
    }
}
