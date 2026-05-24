using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Competitions.Application.Dtos;
using TheLeague.Modules.Competitions.Infrastructure.Persistence;
using TheLeague.Modules.Competitions.Infrastructure.Services;
using TheLeague.Shared.Contracts.Events;
using TheLeague.Shared.Contracts.Messaging;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Enums;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Competitions.Application.Commands;

public record RecordMatchResultCommand(
    Guid MatchId,
    string HomeScore,
    string AwayScore,
    MatchResult Result
) : IRequest<Result<MatchDto>>;

public class RecordMatchResultCommandHandler : IRequestHandler<RecordMatchResultCommand, Result<MatchDto>>
{
    private readonly CompetitionsDbContext _db;
    private readonly ITenantService _tenantService;
    private readonly StandingsCalculator _standingsCalculator;
    private readonly IIntegrationEventBus _eventBus;

    public RecordMatchResultCommandHandler(
        CompetitionsDbContext db,
        ITenantService tenantService,
        StandingsCalculator standingsCalculator,
        IIntegrationEventBus eventBus)
    {
        _db = db;
        _tenantService = tenantService;
        _standingsCalculator = standingsCalculator;
        _eventBus = eventBus;
    }

    public async Task<Result<MatchDto>> Handle(RecordMatchResultCommand request, CancellationToken cancellationToken)
    {
        var clubId = _tenantService.CurrentTenantId!.Value;

        var match = await _db.Matches
            .FirstOrDefaultAsync(m => m.Id == request.MatchId, cancellationToken);

        if (match == null)
            return Result.Failure<MatchDto>("Match not found.");

        try
        {
            match.Complete(request.HomeScore, request.AwayScore, request.Result);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<MatchDto>(ex.Message);
        }

        // Recalculate standings
        var competition = await _db.Competitions
            .FirstOrDefaultAsync(c => c.Id == match.CompetitionId, cancellationToken);

        if (competition != null)
        {
            var teams = await _db.CompetitionTeams
                .Where(t => t.CompetitionId == competition.Id)
                .ToListAsync(cancellationToken);

            var completedMatches = await _db.Matches
                .Where(m => m.CompetitionId == competition.Id &&
                           (m.Status == MatchStatus.Completed || m.Status == MatchStatus.Walkover))
                .ToListAsync(cancellationToken);

            var newStandings = _standingsCalculator.Calculate(
                clubId, competition.Id, teams, completedMatches,
                competition.PointsForWin, competition.PointsForDraw, competition.PointsForLoss);

            // Remove old standings
            var existingStandings = await _db.CompetitionStandings
                .Where(s => s.CompetitionId == competition.Id)
                .ToListAsync(cancellationToken);

            _db.CompetitionStandings.RemoveRange(existingStandings);
            _db.CompetitionStandings.AddRange(newStandings);
        }

        await _db.SaveChangesAsync(cancellationToken);

        // Publish events
        await _eventBus.PublishAsync(
            new MatchCompletedEvent(match.Id, match.CompetitionId, clubId), cancellationToken);
        await _eventBus.PublishAsync(
            new StandingsUpdatedEvent(match.CompetitionId, clubId), cancellationToken);

        var dto = new MatchDto(
            match.Id, match.CompetitionId, match.RoundNumber,
            match.HomeTeamId, match.AwayTeamId, match.VenueId, match.VenueName,
            match.ScheduledDateTime, match.Status, match.HomeScore, match.AwayScore, match.Result);

        return Result.Success(dto);
    }
}
