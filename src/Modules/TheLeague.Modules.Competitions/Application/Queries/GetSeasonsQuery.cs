using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Competitions.Application.Dtos;
using TheLeague.Modules.Competitions.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Competitions.Application.Queries;

public record GetSeasonsQuery : IRequest<List<SeasonDto>>;

public class GetSeasonsQueryHandler : IRequestHandler<GetSeasonsQuery, List<SeasonDto>>
{
    private readonly CompetitionsDbContext _db;

    public GetSeasonsQueryHandler(CompetitionsDbContext db)
    {
        _db = db;
    }

    public async Task<List<SeasonDto>> Handle(GetSeasonsQuery request, CancellationToken cancellationToken)
    {
        return await _db.Seasons
            .OrderByDescending(s => s.StartDate)
            .Select(s => new SeasonDto(
                s.Id, s.ClubId, s.Name,
                s.StartDate, s.EndDate, s.IsActive, s.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
