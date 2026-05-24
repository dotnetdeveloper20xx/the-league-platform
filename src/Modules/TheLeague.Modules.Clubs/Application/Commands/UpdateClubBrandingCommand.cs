using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Clubs.Application.Dtos;
using TheLeague.Modules.Clubs.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Clubs.Application.Commands;

public record UpdateClubBrandingCommand(
    Guid Id,
    string PrimaryColor,
    string SecondaryColor,
    string? AccentColor,
    string? LogoUrl
) : IRequest<Result<ClubDto>>;

public class UpdateClubBrandingCommandHandler : IRequestHandler<UpdateClubBrandingCommand, Result<ClubDto>>
{
    private readonly ClubsDbContext _db;

    public UpdateClubBrandingCommandHandler(ClubsDbContext db)
    {
        _db = db;
    }

    public async Task<Result<ClubDto>> Handle(UpdateClubBrandingCommand request, CancellationToken cancellationToken)
    {
        var club = await _db.Clubs.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (club is null)
            return Result.Failure<ClubDto>("Club not found.");

        club.UpdateBranding(request.PrimaryColor, request.SecondaryColor, request.AccentColor, request.LogoUrl);

        await _db.SaveChangesAsync(cancellationToken);

        var dto = new ClubDto(
            club.Id, club.Name, club.Slug, club.Description, club.LogoUrl,
            club.PrimaryColor, club.SecondaryColor, club.AccentColor,
            club.ContactEmail, club.ContactPhone, club.Address, club.Website,
            club.ClubType, club.IsActive, club.CreatedAt, club.UpdatedAt
        );

        return Result.Success(dto);
    }
}
