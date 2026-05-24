using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Events.Application.Dtos;
using TheLeague.Modules.Events.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Enums;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Events.Application.Commands;

public record UpdateEventCommand(
    Guid EventId,
    string Title,
    string? Description,
    EventType EventType,
    DateTime StartDateTime,
    DateTime EndDateTime,
    Guid? VenueId,
    string? VenueName,
    int? Capacity,
    bool IsTicketed,
    decimal? StandardPrice,
    decimal? MemberPrice,
    bool AllowRsvp,
    int CancellationDeadlineHours = 48
) : IRequest<Result<EventDto>>;

public class UpdateEventCommandHandler : IRequestHandler<UpdateEventCommand, Result<EventDto>>
{
    private readonly EventsDbContext _db;

    public UpdateEventCommandHandler(EventsDbContext db)
    {
        _db = db;
    }

    public async Task<Result<EventDto>> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
    {
        var evt = await _db.Events.FirstOrDefaultAsync(e => e.Id == request.EventId, cancellationToken);
        if (evt is null)
            return Result.Failure<EventDto>("Event not found.");

        evt.Update(
            request.Title,
            request.Description,
            request.EventType,
            request.StartDateTime,
            request.EndDateTime,
            request.VenueId,
            request.VenueName,
            request.Capacity,
            request.IsTicketed,
            request.StandardPrice,
            request.MemberPrice,
            request.AllowRsvp,
            request.CancellationDeadlineHours);

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
