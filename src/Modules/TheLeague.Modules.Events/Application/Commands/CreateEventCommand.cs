using MediatR;
using TheLeague.Modules.Events.Application.Dtos;
using TheLeague.Modules.Events.Domain;
using TheLeague.Modules.Events.Infrastructure.Persistence;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Enums;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Events.Application.Commands;

public record CreateEventCommand(
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

public class CreateEventCommandHandler : IRequestHandler<CreateEventCommand, Result<EventDto>>
{
    private readonly EventsDbContext _db;
    private readonly ITenantService _tenantService;

    public CreateEventCommandHandler(EventsDbContext db, ITenantService tenantService)
    {
        _db = db;
        _tenantService = tenantService;
    }

    public async Task<Result<EventDto>> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        var clubId = _tenantService.CurrentTenantId!.Value;

        var evt = Event.Create(clubId, request.Title, request.EventType, request.StartDateTime, request.EndDateTime);

        // Update with full details (event is in Draft so update is allowed)
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

        _db.Events.Add(evt);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(MapToDto(evt));
    }

    private static EventDto MapToDto(Event e) => new(
        e.Id, e.ClubId, e.Title, e.Description, e.EventType, e.Status,
        e.StartDateTime, e.EndDateTime, e.VenueId, e.VenueName,
        e.Capacity, e.CurrentRegistrationCount, e.IsTicketed,
        e.StandardPrice, e.MemberPrice, e.AllowRsvp,
        e.CancellationDeadlineHours, e.CreatedAt, e.UpdatedAt);
}
