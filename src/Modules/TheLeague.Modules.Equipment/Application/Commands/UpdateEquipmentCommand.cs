using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Equipment.Application.Dtos;
using TheLeague.Modules.Equipment.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Enums;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Equipment.Application.Commands;

public record UpdateEquipmentCommand(
    Guid Id,
    string Name,
    EquipmentCategory Category,
    EquipmentCondition Condition,
    string Location,
    DateTime? PurchaseDate,
    decimal Value,
    decimal AnnualDepreciationRate,
    string? SerialNumber,
    bool IsActive
) : IRequest<Result<EquipmentDto>>;

public class UpdateEquipmentCommandHandler : IRequestHandler<UpdateEquipmentCommand, Result<EquipmentDto>>
{
    private readonly EquipmentDbContext _db;

    public UpdateEquipmentCommandHandler(EquipmentDbContext db)
    {
        _db = db;
    }

    public async Task<Result<EquipmentDto>> Handle(UpdateEquipmentCommand request, CancellationToken cancellationToken)
    {
        var equipment = await _db.Equipment.FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
        if (equipment is null)
            return Result.Failure<EquipmentDto>("Equipment not found.");

        equipment.Update(
            request.Name,
            request.Category,
            request.Condition,
            request.Location,
            request.PurchaseDate,
            request.Value,
            request.AnnualDepreciationRate,
            request.SerialNumber,
            request.IsActive);

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(new EquipmentDto(
            equipment.Id, equipment.Name, equipment.Category, equipment.Condition,
            equipment.Location, equipment.PurchaseDate, equipment.Value,
            equipment.AnnualDepreciationRate, equipment.SerialNumber,
            equipment.IsActive, equipment.CreatedAt, equipment.UpdatedAt));
    }
}
