using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Competitions.Application.Dtos;
using TheLeague.Modules.Competitions.Infrastructure.Persistence;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Enums;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Competitions.Application.Commands;

public record UpdateMatchStatusCommand(
    Guid MatchId,
    MatchStatus NewStatus,
    Guid? WinningTeamId = null
) : IRequest<Result<MatchDto>>;

public class UpdateMatchStatusCommandHandler : IRequestHandler<UpdateMatchStatusCommand, Result<MatchDto>>
{
    private readonly CompetitionsDbContext _db;
    private readonly ITenantService _tenantService;

    public UpdateMatchStatusCommandHandler(CompetitionsDbContext db, ITenantService tenantService)
    {
        _db = db;
        _tenantService = tenantService;
    }

    public async Task<Result<MatchDto>> Handle(UpdateMatchStatusCommand request, CancellationToken cancellationToken)
    {
        var match = await _db.Matches
            .FirstOrDefaultAsync(m => m.Id == request.MatchId, cancellationToken);

        if (match == null)
            return Result.Failure<MatchDto>("Match not found.");

        try
        {
            switch (request.NewStatus)
            {
                case MatchStatus.Confirmed:
                    match.Confirm();
                    break;
                case MatchStatus.InProgress:
                    match.Start();
                    break;
                case MatchStatus.Postponed:
                    match.Postpone();
                    break;
                case MatchStatus.Cancelled:
                    match.Cancel();
                    break;
                case MatchStatus.Abandoned:
                    match.Abandon();
                    break;
                case MatchStatus.Walkover:
                    if (request.WinningTeamId == null)
                        return Result.Failure<MatchDto>("WinningTeamId is required for walkover.");
                    match.Walkover(request.WinningTeamId.Value);
                    break;
                default:
                    return Result.Failure<MatchDto>($"Cannot directly set status to '{request.NewStatus}'. Use the appropriate action.");
            }
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<MatchDto>(ex.Message);
        }

        await _db.SaveChangesAsync(cancellationToken);

        var dto = new MatchDto(
            match.Id, match.CompetitionId, match.RoundNumber,
            match.HomeTeamId, match.AwayTeamId, match.VenueId, match.VenueName,
            match.ScheduledDateTime, match.Status, match.HomeScore, match.AwayScore, match.Result);

        return Result.Success(dto);
    }
}
