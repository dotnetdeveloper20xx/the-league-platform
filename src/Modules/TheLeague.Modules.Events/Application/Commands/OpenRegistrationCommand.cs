using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Events.Application.Dtos;
using TheLeague.Modules.Events.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Events.Application.Commands;

public record OpenRegistrationCommand(Guid EventId) : IRequest<Result<EventDto>>;

public class OpenRegistrationCommandHandler : IRequestHandler<OpenRegistrationCommand, Result<EventDto>>
{
    private readonly EventsDbContext _db;

    public OpenRegistrationCommandHandler(EventsDbContext db)
    {
        _db = db;
    }

    public async Task<Result<EventDto>> Handle(OpenRegistrationCommand request, CancellationToken cancellationToken)
    {
        var evt = await _db.Events.FirstOrDefaultAsync(e => e.Id == request.EventId, cancellationToken);
        if (evt is null)
            return Result.Failure<EventDto>("Event not found.");

        evt.OpenRegistration();
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(MapToDto(evt));
    }

    private static EventDto MapToDto(Domain.Event e) => new(
        e.Id, e.ClubId, e.Title, e.Description, e.EventType, e.Status,
        e.StartDateTime, e.EndDateTime, e.VenueId, e.VenueName,
        e.Capacity, e.CurrentRegistrationCount, e.IsTicketed,
        e.StandardPrice, e.MemberPrice, e.AllowRsvp,
        e.CancellationDeadlineHours, e.CreatedAt, e.UpdatedAt);
}
