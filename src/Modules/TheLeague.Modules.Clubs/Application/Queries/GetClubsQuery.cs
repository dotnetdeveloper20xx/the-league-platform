using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Clubs.Application.Dtos;
using TheLeague.Modules.Clubs.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Clubs.Application.Queries;

public record GetClubsQuery(int Page = 1, int PageSize = 20) : IRequest<PagedResult<ClubDto>>;

public class GetClubsQueryHandler : IRequestHandler<GetClubsQuery, PagedResult<ClubDto>>
{
    private readonly ClubsDbContext _db;

    public GetClubsQueryHandler(ClubsDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<ClubDto>> Handle(GetClubsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Clubs.AsNoTracking().OrderBy(c => c.Name);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(club => new ClubDto(
                club.Id, club.Name, club.Slug, club.Description, club.LogoUrl,
                club.PrimaryColor, club.SecondaryColor, club.AccentColor,
                club.ContactEmail, club.ContactPhone, club.Address, club.Website,
                club.ClubType, club.IsActive, club.CreatedAt, club.UpdatedAt
            ))
            .ToListAsync(cancellationToken);

        return new PagedResult<ClubDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
