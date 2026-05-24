using MediatR;
using TheLeague.Modules.Sessions.Application.Dtos;
using TheLeague.Modules.Sessions.Domain;
using TheLeague.Modules.Sessions.Infrastructure.Persistence;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Enums;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Sessions.Application.Commands;

public record CreateSessionCommand(
    string Title,
    SessionCategory Category,
    Guid? VenueId,
    string? VenueName,
    DateTime StartTime,
    int Duration,
    int Capacity,
    decimal Fee,
    int CancellationDeadlineHours = 24
) : IRequest<Result<SessionDto>>;

public class CreateSessionCommandHandler : IRequestHandler<CreateSessionCommand, Result<SessionDto>>
{
    private readonly SessionsDbContext _db;
    private readonly ITenantService _tenantService;

    public CreateSessionCommandHandler(SessionsDbContext db, ITenantService tenantService)
    {
        _db = db;
        _tenantService = tenantService;
    }

    public async Task<Result<SessionDto>> Handle(CreateSessionCommand request, CancellationToken cancellationToken)
    {
        var clubId = _tenantService.CurrentTenantId;
        if (clubId is null)
            return Result.Failure<SessionDto>("Tenant context is required.");

        var session = Session.Create(
            clubId.Value,
            request.Title,
            request.Category,
            request.VenueId,
            request.VenueName,
            request.StartTime,
            request.Duration,
            request.Capacity,
            request.Fee,
            request.CancellationDeadlineHours);

        _db.Sessions.Add(session);
        await _db.SaveChangesAsync(cancellationToken);

        var dto = new SessionDto(
            session.Id, session.Title, session.Category,
            session.VenueId, session.VenueName,
            session.StartTime, session.EndTime, session.Duration,
            session.Capacity, session.Fee, session.CurrentBookingCount,
            session.Capacity - session.CurrentBookingCount,
            session.IsCancelled, session.CancellationReason,
            session.CancellationDeadlineHours, session.CreatedAt);

        return Result.Success(dto);
    }
}
