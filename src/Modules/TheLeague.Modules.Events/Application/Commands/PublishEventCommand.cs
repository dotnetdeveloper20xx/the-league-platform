using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Events.Application.Dtos;
using TheLeague.Modules.Events.Infrastructure.Persistence;
using TheLeague.Shared.Contracts.Events;
using TheLeague.Shared.Contracts.Messaging;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Events.Application.Commands;

public record PublishEventCommand(Guid EventId) : IRequest<Result<EventDto>>;

public class PublishEventCommandHandler : IRequestHandler<PublishEventCommand, Result<EventDto>>
{
    private readonly EventsDbContext _db;
    private readonly IIntegrationEventBus _eventBus;

    public PublishEventCommandHandler(EventsDbContext db, IIntegrationEventBus eventBus)
    {
        _db = db;
        _eventBus = eventBus;
    }

    public async Task<Result<EventDto>> Handle(PublishEventCommand request, CancellationToken cancellationToken)
    {
        var evt = await _db.Events.FirstOrDefaultAsync(e => e.Id == request.EventId, cancellationToken);
        if (evt is null)
            return Result.Failure<EventDto>("Event not found.");

        evt.Publish();
        await _db.SaveChangesAsync(cancellationToken);

        await _eventBus.PublishAsync(
            new EventPublishedEvent(evt.Id, evt.ClubId, evt.Title),
            cancellationToken);

        return Result.Success(MapToDto(evt));
    }

    private static EventDto MapToDto(Domain.Event e) => new(
        e.Id, e.ClubId, e.Title, e.Description, e.EventType, e.Status,
        e.StartDateTime, e.EndDateTime, e.VenueId, e.VenueName,
        e.Capacity, e.CurrentRegistrationCount, e.IsTicketed,
        e.StandardPrice, e.MemberPrice, e.AllowRsvp,
        e.CancellationDeadlineHours, e.CreatedAt, e.UpdatedAt);
}
