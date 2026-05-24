using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Competitions.Application.Dtos;
using TheLeague.Modules.Competitions.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Competitions.Application.Queries;

public record GetMatchByIdQuery(Guid Id) : IRequest<Result<MatchDetailDto>>;

public class GetMatchByIdQueryHandler : IRequestHandler<GetMatchByIdQuery, Result<MatchDetailDto>>
{
    private readonly CompetitionsDbContext _db;

    public GetMatchByIdQueryHandler(CompetitionsDbContext db)
    {
        _db = db;
    }

    public async Task<Result<MatchDetailDto>> Handle(GetMatchByIdQuery request, CancellationToken cancellationToken)
    {
        var match = await _db.Matches
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (match == null)
            return Result.Failure<MatchDetailDto>("Match not found.");

        var events = await _db.MatchEvents
            .Where(e => e.MatchId == request.Id)
            .OrderBy(e => e.Minute)
            .ThenBy(e => e.Timestamp)
            .Select(e => new MatchEventDto(
                e.Id, e.MatchId, e.TeamId, e.PlayerId,
                e.EventType, e.Minute, e.Description, e.Timestamp))
            .ToListAsync(cancellationToken);

        var lineups = await _db.MatchLineups
            .Where(l => l.MatchId == request.Id)
            .OrderBy(l => l.TeamId)
            .ThenByDescending(l => l.IsStarter)
            .Select(l => new MatchLineupDto(
                l.Id, l.MatchId, l.TeamId, l.PlayerId, l.IsStarter, l.Position))
            .ToListAsync(cancellationToken);

        var dto = new MatchDetailDto(
            match.Id, match.CompetitionId, match.RoundNumber,
            match.HomeTeamId, match.AwayTeamId, match.VenueId, match.VenueName,
            match.ScheduledDateTime, match.Status, match.HomeScore, match.AwayScore,
            match.Result, events, lineups);

        return Result.Success(dto);
    }
}
