using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Clubs.Application.Dtos;
using TheLeague.Modules.Clubs.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Clubs.Application.Commands;

public record UpdateClubCommand(
    Guid Id,
    string Name,
    string? Description,
    string? ContactEmail,
    string? ContactPhone,
    string? Address,
    string? Website
) : IRequest<Result<ClubDto>>;

public class UpdateClubCommandHandler : IRequestHandler<UpdateClubCommand, Result<ClubDto>>
{
    private readonly ClubsDbContext _db;

    public UpdateClubCommandHandler(ClubsDbContext db)
    {
        _db = db;
    }

    public async Task<Result<ClubDto>> Handle(UpdateClubCommand request, CancellationToken cancellationToken)
    {
        var club = await _db.Clubs.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (club is null)
            return Result.Failure<ClubDto>("Club not found.");

        club.Update(request.Name, request.Description, request.ContactEmail,
            request.ContactPhone, request.Address, request.Website);

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
