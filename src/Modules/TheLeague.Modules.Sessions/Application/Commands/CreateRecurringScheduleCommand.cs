using MediatR;
using TheLeague.Modules.Sessions.Application.Dtos;
using TheLeague.Modules.Sessions.Domain;
using TheLeague.Modules.Sessions.Infrastructure.Persistence;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Enums;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Sessions.Application.Commands;

public record CreateRecurringScheduleCommand(
    string Title,
    SessionCategory Category,
    Guid? VenueId,
    string? VenueName,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    int Duration,
    int Capacity,
    decimal Fee,
    int HorizonWeeks
) : IRequest<Result<RecurringScheduleDto>>;

public class CreateRecurringScheduleCommandHandler : IRequestHandler<CreateRecurringScheduleCommand, Result<RecurringScheduleDto>>
{
    private readonly SessionsDbContext _db;
    private readonly ITenantService _tenantService;

    public CreateRecurringScheduleCommandHandler(SessionsDbContext db, ITenantService tenantService)
    {
        _db = db;
        _tenantService = tenantService;
    }

    public async Task<Result<RecurringScheduleDto>> Handle(CreateRecurringScheduleCommand request, CancellationToken cancellationToken)
    {
        var clubId = _tenantService.CurrentTenantId;
        if (clubId is null)
            return Result.Failure<RecurringScheduleDto>("Tenant context is required.");

        var schedule = RecurringSchedule.Create(
            clubId.Value,
            request.Title,
            request.Category,
            request.VenueId,
            request.VenueName,
            request.DayOfWeek,
            request.StartTime,
            request.Duration,
            request.Capacity,
            request.Fee,
            request.HorizonWeeks);

        _db.RecurringSchedules.Add(schedule);

        // Generate sessions for the horizon
        var sessions = schedule.GenerateSessions(DateTime.UtcNow);
        _db.Sessions.AddRange(sessions);

        await _db.SaveChangesAsync(cancellationToken);

        var dto = new RecurringScheduleDto(
            schedule.Id, schedule.Title, schedule.Category,
            schedule.VenueId, schedule.VenueName,
            schedule.DayOfWeek, schedule.StartTime, schedule.Duration,
            schedule.Capacity, schedule.Fee, schedule.HorizonWeeks,
            schedule.IsActive, schedule.CreatedAt);

        return Result.Success(dto);
    }
}
