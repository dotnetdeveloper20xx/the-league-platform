using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Clubs.Application.Dtos;
using TheLeague.Modules.Clubs.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Clubs.Application.Queries;

public record GetClubSettingsQuery(Guid ClubId) : IRequest<Result<ClubSettingsDto>>;

public class GetClubSettingsQueryHandler : IRequestHandler<GetClubSettingsQuery, Result<ClubSettingsDto>>
{
    private readonly ClubsDbContext _db;

    public GetClubSettingsQueryHandler(ClubsDbContext db)
    {
        _db = db;
    }

    public async Task<Result<ClubSettingsDto>> Handle(GetClubSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await _db.ClubSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.ClubId == request.ClubId, cancellationToken);

        if (settings is null)
            return Result.Failure<ClubSettingsDto>("Club settings not found.");

        var dto = new ClubSettingsDto(
            settings.Id, settings.ClubId, settings.Timezone, settings.Currency,
            settings.Locale, settings.BookingCancellationHours,
            settings.RequireEmailVerification, settings.CustomTerminology
        );

        return Result.Success(dto);
    }
}
