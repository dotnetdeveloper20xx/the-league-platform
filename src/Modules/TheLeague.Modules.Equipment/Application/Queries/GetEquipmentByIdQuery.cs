using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Equipment.Application.Dtos;
using TheLeague.Modules.Equipment.Domain;
using TheLeague.Modules.Equipment.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Equipment.Application.Queries;

public record GetEquipmentByIdQuery(Guid Id) : IRequest<Result<EquipmentDetailDto>>;

public class GetEquipmentByIdQueryHandler : IRequestHandler<GetEquipmentByIdQuery, Result<EquipmentDetailDto>>
{
    private readonly EquipmentDbContext _db;

    public GetEquipmentByIdQueryHandler(EquipmentDbContext db)
    {
        _db = db;
    }

    public async Task<Result<EquipmentDetailDto>> Handle(GetEquipmentByIdQuery request, CancellationToken cancellationToken)
    {
        var equipment = await _db.Equipment
            .AsNoTracking()
            .Include(e => e.Loans.OrderByDescending(l => l.LoanDate))
            .Include(e => e.Reservations.OrderByDescending(r => r.StartDate))
            .Include(e => e.MaintenanceRecords.OrderByDescending(m => m.MaintenanceDate))
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (equipment is null)
            return Result.Failure<EquipmentDetailDto>("Equipment not found.");

        var dto = new EquipmentDetailDto(
            equipment.Id, equipment.Name, equipment.Category, equipment.Condition,
            equipment.Location, equipment.PurchaseDate, equipment.Value,
            equipment.AnnualDepreciationRate, equipment.SerialNumber,
            equipment.IsActive, equipment.CreatedAt, equipment.UpdatedAt,
            equipment.Loans.Select(l => new LoanDto(
                l.Id, l.EquipmentId, l.MemberId, l.Status,
                l.LoanDate, l.ExpectedReturnDate, l.ActualReturnDate,
                l.Fee, l.Deposit, l.Notes, l.CreatedAt)).ToList(),
            equipment.Reservations.Select(r => new ReservationDto(
                r.Id, r.EquipmentId, r.MemberId,
                r.StartDate, r.EndDate, r.Status,
                r.Notes, r.CreatedAt)).ToList(),
            equipment.MaintenanceRecords.Select(m => new MaintenanceDto(
                m.Id, m.EquipmentId, m.MaintenanceDate,
                m.Description, m.ResultingCondition,
                m.Cost, m.PerformedBy, m.CreatedAt)).ToList());

        return Result.Success(dto);
    }
}
