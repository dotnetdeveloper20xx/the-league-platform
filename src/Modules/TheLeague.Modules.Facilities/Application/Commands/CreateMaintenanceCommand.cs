using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Facilities.Domain;
using TheLeague.Modules.Facilities.Infrastructure.Persistence;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Facilities.Application.Commands;

public record CreateMaintenanceCommand(
    Guid FacilityId,
    string Title,
    string? Description,
    DateTime StartDate,
    DateTime EndDate
) : IRequest<Result<Guid>>;

public class CreateMaintenanceCommandHandler : IRequestHandler<CreateMaintenanceCommand, Result<Guid>>
{
    private readonly FacilitiesDbContext _db;
    private readonly ITenantService _tenantService;

    public CreateMaintenanceCommandHandler(FacilitiesDbContext db, ITenantService tenantService)
    {
        _db = db;
        _tenantService = tenantService;
    }

    public async Task<Result<Guid>> Handle(CreateMaintenanceCommand request, CancellationToken cancellationToken)
    {
        var clubId = _tenantService.CurrentTenantId;
        if (clubId == null)
            return Result.Failure<Guid>("Tenant context is required.");

        var facility = await _db.Facilities.FirstOrDefaultAsync(f => f.Id == request.FacilityId, cancellationToken);
        if (facility == null)
            return Result.Failure<Guid>("Facility not found.");

        if (request.EndDate <= request.StartDate)
            return Result.Failure<Guid>("End date must be after start date.");

        // Auto-cancel conflicting bookings
        var conflictingBookings = await _db.FacilityBookings
            .Where(b => b.FacilityId == request.FacilityId
                && b.Status == BookingStatus.Confirmed
                && b.BookingDate >= DateOnly.FromDateTime(request.StartDate)
                && b.BookingDate <= DateOnly.FromDateTime(request.EndDate))
            .ToListAsync(cancellationToken);

        // Filter to only those that actually overlap with the maintenance window
        var overlapping = conflictingBookings.Where(b =>
        {
            var bookingStart = b.BookingDate.ToDateTime(b.StartTime);
            var bookingEnd = b.BookingDate.ToDateTime(b.EndTime);
            return bookingStart < request.EndDate && bookingEnd > request.StartDate;
        }).ToList();

        foreach (var booking in overlapping)
        {
            booking.Cancel();
        }

        var maintenance = FacilityMaintenance.Create(
            clubId.Value, request.FacilityId, request.Title,
            request.Description, request.StartDate, request.EndDate);

        _db.FacilityMaintenances.Add(maintenance);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(maintenance.Id);
    }
}
