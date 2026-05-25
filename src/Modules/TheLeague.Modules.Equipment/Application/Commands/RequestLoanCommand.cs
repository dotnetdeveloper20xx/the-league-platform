using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Equipment.Application.Dtos;
using TheLeague.Modules.Equipment.Domain;
using TheLeague.Modules.Equipment.Infrastructure.Persistence;
using TheLeague.Shared.Contracts.Events;
using TheLeague.Shared.Contracts.Messaging;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Enums;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Equipment.Application.Commands;

public record RequestLoanCommand(
    Guid EquipmentId,
    Guid MemberId,
    DateTime LoanDate,
    DateTime ExpectedReturnDate,
    decimal Fee,
    decimal Deposit,
    string? Notes
) : IRequest<Result<LoanDto>>;

public class RequestLoanCommandHandler : IRequestHandler<RequestLoanCommand, Result<LoanDto>>
{
    private readonly EquipmentDbContext _db;
    private readonly ITenantService _tenantService;
    private readonly IIntegrationEventBus _eventBus;

    public RequestLoanCommandHandler(EquipmentDbContext db, ITenantService tenantService, IIntegrationEventBus eventBus)
    {
        _db = db;
        _tenantService = tenantService;
        _eventBus = eventBus;
    }

    public async Task<Result<LoanDto>> Handle(RequestLoanCommand request, CancellationToken cancellationToken)
    {
        var clubId = _tenantService.CurrentTenantId!.Value;

        var equipment = await _db.Equipment.FirstOrDefaultAsync(e => e.Id == request.EquipmentId, cancellationToken);
        if (equipment is null)
            return Result.Failure<LoanDto>("Equipment not found.");

        // Validate condition - cannot loan if NeedsRepair, Damaged, or Decommissioned
        if (!equipment.IsLoanable())
            return Result.Failure<LoanDto>($"Equipment cannot be loaned. Current condition: {equipment.Condition}");

        // Check for overlapping active loans
        var existingLoans = await _db.EquipmentLoans
            .Where(l => l.EquipmentId == request.EquipmentId)
            .Where(l => l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue || l.Status == LoanStatus.Approved || l.Status == LoanStatus.Requested)
            .ToListAsync(cancellationToken);

        var hasOverlap = existingLoans.Any(l => l.OverlapsWith(request.LoanDate, request.ExpectedReturnDate));
        if (hasOverlap)
            return Result.Failure<LoanDto>("Equipment is already on loan during the requested period.");

        // Check for overlapping reservations
        var existingReservations = await _db.EquipmentReservations
            .Where(r => r.EquipmentId == request.EquipmentId)
            .Where(r => r.Status != ReservationStatus.Cancelled)
            .ToListAsync(cancellationToken);

        var hasReservationOverlap = existingReservations.Any(r => r.OverlapsWith(request.LoanDate, request.ExpectedReturnDate));
        if (hasReservationOverlap)
            return Result.Failure<LoanDto>("Equipment has a reservation during the requested period.");

        var loan = EquipmentLoan.Create(
            clubId,
            request.EquipmentId,
            request.MemberId,
            request.LoanDate,
            request.ExpectedReturnDate,
            request.Fee,
            request.Deposit,
            request.Notes);

        _db.EquipmentLoans.Add(loan);
        await _db.SaveChangesAsync(cancellationToken);

        // Publish overdue event if already overdue at creation (edge case)
        if (loan.IsOverdue(DateTime.UtcNow))
        {
            await _eventBus.PublishAsync(
                new LoanOverdueEvent(loan.Id, loan.EquipmentId, loan.MemberId, clubId),
                cancellationToken);
        }

        return Result.Success(new LoanDto(
            loan.Id, loan.EquipmentId, loan.MemberId, loan.Status,
            loan.LoanDate, loan.ExpectedReturnDate, loan.ActualReturnDate,
            loan.Fee, loan.Deposit, loan.Notes, loan.CreatedAt));
    }
}
