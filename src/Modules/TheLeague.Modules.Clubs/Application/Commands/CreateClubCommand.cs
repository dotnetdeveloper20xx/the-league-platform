using MediatR;
using TheLeague.Modules.Clubs.Application.Dtos;
using TheLeague.Modules.Clubs.Domain;
using TheLeague.Modules.Clubs.Infrastructure.Persistence;
using TheLeague.Shared.Contracts.Events;
using TheLeague.Shared.Contracts.Messaging;
using TheLeague.Shared.Domain.Enums;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Clubs.Application.Commands;

public record CreateClubCommand(
    string Name,
    string Slug,
    ClubType ClubType
) : IRequest<Result<ClubDto>>;

public class CreateClubCommandHandler : IRequestHandler<CreateClubCommand, Result<ClubDto>>
{
    private readonly ClubsDbContext _db;
    private readonly IIntegrationEventBus _eventBus;

    public CreateClubCommandHandler(ClubsDbContext db, IIntegrationEventBus eventBus)
    {
        _db = db;
        _eventBus = eventBus;
    }

    public async Task<Result<ClubDto>> Handle(CreateClubCommand request, CancellationToken cancellationToken)
    {
        var club = Club.Create(request.Name, request.Slug, request.ClubType);

        var settings = ClubSettings.CreateDefault(club.Id);
        var sportConfig = SportConfiguration.CreateForSport(club.Id, request.ClubType);

        _db.Clubs.Add(club);
        _db.ClubSettings.Add(settings);
        _db.SportConfigurations.Add(sportConfig);

        await _db.SaveChangesAsync(cancellationToken);

        await _eventBus.PublishAsync(new ClubCreatedEvent(club.Id, club.Name, club.Slug), cancellationToken);

        var dto = new ClubDto(
            club.Id, club.Name, club.Slug, club.Description, club.LogoUrl,
            club.PrimaryColor, club.SecondaryColor, club.AccentColor,
            club.ContactEmail, club.ContactPhone, club.Address, club.Website,
            club.ClubType, club.IsActive, club.CreatedAt, club.UpdatedAt
        );

        return Result.Success(dto);
    }
}
