using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Competitions.Application.Dtos;
using TheLeague.Modules.Competitions.Infrastructure.Persistence;
using TheLeague.Modules.Competitions.Infrastructure.Services;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Enums;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Competitions.Application.Commands;

public record GenerateFixturesCommand(
    Guid CompetitionId,
    DateTime StartDate
) : IRequest<Result<List<MatchDto>>>;

public class GenerateFixturesCommandHandler : IRequestHandler<GenerateFixturesCommand, Result<List<MatchDto>>>
{
    private readonly CompetitionsDbContext _db;
    private readonly ITenantService _tenantService;
    private readonly FixtureGenerator _fixtureGenerator;

    public GenerateFixturesCommandHandler(
        CompetitionsDbContext db,
        ITenantService tenantService,
        FixtureGenerator fixtureGenerator)
    {
        _db = db;
        _tenantService = tenantService;
        _fixtureGenerator = fixtureGenerator;
    }

    public async Task<Result<List<MatchDto>>> Handle(GenerateFixturesCommand request, CancellationToken cancellationToken)
    {
        var clubId = _tenantService.CurrentTenantId!.Value;

        var competition = await _db.Competitions
            .FirstOrDefaultAsync(c => c.Id == request.CompetitionId, cancellationToken);

        if (competition == null)
            return Result.Failure<List<MatchDto>>("Competition not found.");

        var teams = await _db.CompetitionTeams
            .Where(t => t.CompetitionId == request.CompetitionId)
            .ToListAsync(cancellationToken);

        if (teams.Count < 2)
            return Result.Failure<List<MatchDto>>("At least 2 teams are required to generate fixtures.");

        var matches = competition.CompetitionType switch
        {
            CompetitionType.Knockout => _fixtureGenerator.GenerateKnockout(clubId, competition.Id, teams, request.StartDate),
            _ => _fixtureGenerator.GenerateRoundRobin(clubId, competition.Id, teams, request.StartDate)
        };

        _db.Matches.AddRange(matches);
        await _db.SaveChangesAsync(cancellationToken);

        var dtos = matches.Select(m => new MatchDto(
            m.Id, m.CompetitionId, m.RoundNumber,
            m.HomeTeamId, m.AwayTeamId, m.VenueId, m.VenueName,
            m.ScheduledDateTime, m.Status, m.HomeScore, m.AwayScore, m.Result
        )).ToList();

        return Result.Success(dtos);
    }
}
