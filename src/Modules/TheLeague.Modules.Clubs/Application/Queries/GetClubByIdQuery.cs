using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Clubs.Application.Dtos;
using TheLeague.Modules.Clubs.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Clubs.Application.Queries;

public record GetClubByIdQuery(Guid Id) : IRequest<Result<ClubDto>>;

public class GetClubByIdQueryHandler : IRequestHandler<GetClubByIdQuery, Result<ClubDto>>
{
    private readonly ClubsDbContext _db;

    public GetClubByIdQueryHandler(ClubsDbContext db)
    {
        _db = db;
    }

    public async Task<Result<ClubDto>> Handle(GetClubByIdQuery request, CancellationToken cancellationToken)
    {
        var club = await _db.Clubs
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (club is null)
            return Result.Failure<ClubDto>("Club not found.");

        var dto = new ClubDto(
            club.Id, club.Name, club.Slug, club.Description, club.LogoUrl,
            club.PrimaryColor, club.SecondaryColor, club.AccentColor,
            club.ContactEmail, club.ContactPhone, club.Address, club.Website,
            club.ClubType, club.IsActive, club.CreatedAt, club.UpdatedAt
        );

        return Result.Success(dto);
    }
}
