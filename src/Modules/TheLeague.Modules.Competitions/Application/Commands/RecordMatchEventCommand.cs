using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Competitions.Application.Dtos;
using TheLeague.Modules.Competitions.Domain;
using TheLeague.Modules.Competitions.Infrastructure.Persistence;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Competitions.Application.Commands;

public record RecordMatchEventCommand(
    Guid MatchId,
    Guid TeamId,
    Guid? PlayerId,
    string EventType,
    int? Minute,
    string? Description
) : IRequest<Result<MatchEventDto>>;

public class RecordMatchEventCommandHandler : IRequestHandler<RecordMatchEventCommand, Result<MatchEventDto>>
{
    private readonly CompetitionsDbContext _db;
    private readonly ITenantService _tenantService;

    public RecordMatchEventCommandHandler(CompetitionsDbContext db, ITenantService tenantService)
    {
        _db = db;
        _tenantService = tenantService;
    }

    public async Task<Result<MatchEventDto>> Handle(RecordMatchEventCommand request, CancellationToken cancellationToken)
    {
        var clubId = _tenantService.CurrentTenantId!.Value;

        var match = await _db.Matches
            .FirstOrDefaultAsync(m => m.Id == request.MatchId, cancellationToken);

        if (match == null)
            return Result.Failure<MatchEventDto>("Match not found.");

        // Validate substitution limit (max 5 per team)
        if (request.EventType.Equals("Substitution", StringComparison.OrdinalIgnoreCase))
        {
            var subsCount = await _db.MatchEvents
                .CountAsync(e => e.MatchId == request.MatchId
                              && e.TeamId == request.TeamId
                              && e.EventType == "Substitution", cancellationToken);

            if (subsCount >= 5)
                return Result.Failure<MatchEventDto>("Maximum 5 substitutions per team per match.");
        }

        var matchEvent = MatchEvent.Create(
            clubId,
            request.MatchId,
            request.TeamId,
            request.PlayerId,
            request.EventType,
            request.Minute,
            request.Description);

        _db.MatchEvents.Add(matchEvent);
        await _db.SaveChangesAsync(cancellationToken);

        var dto = new MatchEventDto(
            matchEvent.Id, matchEvent.MatchId, matchEvent.TeamId,
            matchEvent.PlayerId, matchEvent.EventType, matchEvent.Minute,
            matchEvent.Description, matchEvent.Timestamp);

        return Result.Success(dto);
    }
}
