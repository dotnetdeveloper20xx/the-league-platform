using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Competitions.Application.Dtos;
using TheLeague.Modules.Competitions.Domain;
using TheLeague.Modules.Competitions.Infrastructure.Persistence;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Competitions.Application.Commands;

public record RegisterTeamCommand(
    Guid CompetitionId,
    string TeamName,
    Guid? CaptainMemberId,
    Guid? HomeVenueId,
    string? HomeVenueName,
    string? TeamColor,
    List<TeamParticipantRequest> Players
) : IRequest<Result<CompetitionTeamDto>>;

public record TeamParticipantRequest(
    Guid MemberId,
    int? JerseyNumber,
    bool IsCaptain = false
);

public class RegisterTeamCommandHandler : IRequestHandler<RegisterTeamCommand, Result<CompetitionTeamDto>>
{
    private readonly CompetitionsDbContext _db;
    private readonly ITenantService _tenantService;

    public RegisterTeamCommandHandler(CompetitionsDbContext db, ITenantService tenantService)
    {
        _db = db;
        _tenantService = tenantService;
    }

    public async Task<Result<CompetitionTeamDto>> Handle(RegisterTeamCommand request, CancellationToken cancellationToken)
    {
        var clubId = _tenantService.CurrentTenantId!.Value;

        // Validate squad size
        if (request.Players.Count < 11 || request.Players.Count > 30)
            return Result.Failure<CompetitionTeamDto>("Squad size must be between 11 and 30 players.");

        // Validate exactly 1 captain
        var captainCount = request.Players.Count(p => p.IsCaptain);
        if (captainCount != 1)
            return Result.Failure<CompetitionTeamDto>("Exactly one captain must be assigned per team.");

        var captain = request.Players.First(p => p.IsCaptain);

        // Verify competition exists
        var competition = await _db.Competitions.FirstOrDefaultAsync(
            c => c.Id == request.CompetitionId, cancellationToken);

        if (competition == null)
            return Result.Failure<CompetitionTeamDto>("Competition not found.");

        var team = CompetitionTeam.Create(
            clubId,
            request.CompetitionId,
            request.TeamName,
            captain.MemberId,
            request.HomeVenueId,
            request.HomeVenueName,
            request.TeamColor,
            request.Players.Count);

        _db.CompetitionTeams.Add(team);

        // Add participants
        foreach (var player in request.Players)
        {
            var participant = CompetitionParticipant.Create(
                clubId, team.Id, player.MemberId, player.JerseyNumber);
            _db.CompetitionParticipants.Add(participant);
        }

        await _db.SaveChangesAsync(cancellationToken);

        var dto = new CompetitionTeamDto(
            team.Id, team.CompetitionId, team.TeamName,
            team.CaptainMemberId, team.HomeVenueId,
            team.HomeVenueName, team.TeamColor, team.SquadSize);

        return Result.Success(dto);
    }
}
