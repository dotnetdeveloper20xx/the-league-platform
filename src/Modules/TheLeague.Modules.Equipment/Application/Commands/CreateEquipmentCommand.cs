using MediatR;
using TheLeague.Modules.Equipment.Application.Dtos;
using TheLeague.Modules.Equipment.Domain;
using TheLeague.Modules.Equipment.Infrastructure.Persistence;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Enums;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Equipment.Application.Commands;

public record CreateEquipmentCommand(
    string Name,
    EquipmentCategory Category,
    EquipmentCondition Condition,
    string Location,
    DateTime? PurchaseDate,
    decimal Value,
    decimal AnnualDepreciationRate,
    string? SerialNumber
) : IRequest<Result<EquipmentDto>>;

public class CreateEquipmentCommandHandler : IRequestHandler<CreateEquipmentCommand, Result<EquipmentDto>>
{
    private readonly EquipmentDbContext _db;
    private readonly ITenantService _tenantService;

    public CreateEquipmentCommandHandler(EquipmentDbContext db, ITenantService tenantService)
    {
        _db = db;
        _tenantService = tenantService;
    }

    public async Task<Result<EquipmentDto>> Handle(CreateEquipmentCommand request, CancellationToken cancellationToken)
    {
        var clubId = _tenantService.CurrentTenantId!.Value;

        var equipment = EquipmentItem.Create(
            clubId,
            request.Name,
            request.Category,
            request.Condition,
            request.Location,
            request.PurchaseDate,
            request.Value,
            request.AnnualDepreciationRate,
            request.SerialNumber);

        _db.Equipment.Add(equipment);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(MapToDto(equipment));
    }

    private static EquipmentDto MapToDto(EquipmentItem e) => new(
        e.Id, e.Name, e.Category, e.Condition, e.Location,
        e.PurchaseDate, e.Value, e.AnnualDepreciationRate,
        e.SerialNumber, e.IsActive, e.CreatedAt, e.UpdatedAt);
}
