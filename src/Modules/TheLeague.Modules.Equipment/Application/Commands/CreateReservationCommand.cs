using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Equipment.Application.Dtos;
using TheLeague.Modules.Equipment.Domain;
using TheLeague.Modules.Equipment.Infrastructure.Persistence;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Enums;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Equipment.Application.Commands;

public record CreateReservationCommand(
    Guid EquipmentId,
    Guid MemberId,
    DateTime StartDate,
    DateTime EndDate,
    string? Notes
) : IRequest<Result<ReservationDto>>;

public class CreateReservationCommandHandler : IRequestHandler<CreateReservationCommand, Result<ReservationDto>>
{
    private readonly EquipmentDbContext _db;
    private readonly ITenantService _tenantService;

    public CreateReservationCommandHandler(EquipmentDbContext db, ITenantService tenantService)
    {
        _db = db;
        _tenantService = tenantService;
    }

    public async Task<Result<ReservationDto>> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
    {
        var clubId = _tenantService.CurrentTenantId!.Value;

        var equipment = await _db.Equipment.FirstOrDefaultAsync(e => e.Id == request.EquipmentId, cancellationToken);
        if (equipment is null)
            return Result.Failure<ReservationDto>("Equipment not found.");

        // Validate max 365 days advance
        if (request.EndDate > DateTime.UtcNow.AddDays(365))
            return Result.Failure<ReservationDto>("Reservations cannot be made more than 365 days in advance.");

        if (request.StartDate >= request.EndDate)
            return Result.Failure<ReservationDto>("Start date must be before end date.");

        // Check for overlapping active loans
        var existingLoans = await _db.EquipmentLoans
            .Where(l => l.EquipmentId == request.EquipmentId)
            .Where(l => l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue || l.Status == LoanStatus.Approved || l.Status == LoanStatus.Requested)
            .ToListAsync(cancellationToken);

        var hasLoanOverlap = existingLoans.Any(l => l.OverlapsWith(request.StartDate, request.EndDate));
        if (hasLoanOverlap)
            return Result.Failure<ReservationDto>("Equipment has an active loan during the requested period.");

        // Check for overlapping reservations
        var existingReservations = await _db.EquipmentReservations
            .Where(r => r.EquipmentId == request.EquipmentId)
            .Where(r => r.Status != ReservationStatus.Cancelled)
            .ToListAsync(cancellationToken);

        var hasReservationOverlap = existingReservations.Any(r => r.OverlapsWith(request.StartDate, request.EndDate));
        if (hasReservationOverlap)
            return Result.Failure<ReservationDto>("Equipment already has a reservation during the requested period.");

        var reservation = EquipmentReservation.Create(
            clubId,
            request.EquipmentId,
            request.MemberId,
            request.StartDate,
            request.EndDate,
            request.Notes);

        _db.EquipmentReservations.Add(reservation);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(new ReservationDto(
            reservation.Id, reservation.EquipmentId, reservation.MemberId,
            reservation.StartDate, reservation.EndDate, reservation.Status,
            reservation.Notes, reservation.CreatedAt));
    }
}
