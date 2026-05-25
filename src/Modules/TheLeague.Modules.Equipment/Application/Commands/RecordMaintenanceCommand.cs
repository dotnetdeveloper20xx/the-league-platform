using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Equipment.Application.Dtos;
using TheLeague.Modules.Equipment.Domain;
using TheLeague.Modules.Equipment.Infrastructure.Persistence;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Enums;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Equipment.Application.Commands;

public record RecordMaintenanceCommand(
    Guid EquipmentId,
    DateTime MaintenanceDate,
    string Description,
    EquipmentCondition ResultingCondition,
    decimal? Cost,
    string? PerformedBy
) : IRequest<Result<MaintenanceDto>>;

public class RecordMaintenanceCommandHandler : IRequestHandler<RecordMaintenanceCommand, Result<MaintenanceDto>>
{
    private readonly EquipmentDbContext _db;
    private readonly ITenantService _tenantService;

    public RecordMaintenanceCommandHandler(EquipmentDbContext db, ITenantService tenantService)
    {
        _db = db;
        _tenantService = tenantService;
    }

    public async Task<Result<MaintenanceDto>> Handle(RecordMaintenanceCommand request, CancellationToken cancellationToken)
    {
        var clubId = _tenantService.CurrentTenantId!.Value;

        var equipment = await _db.Equipment.FirstOrDefaultAsync(e => e.Id == request.EquipmentId, cancellationToken);
        if (equipment is null)
            return Result.Failure<MaintenanceDto>("Equipment not found.");

        var maintenance = EquipmentMaintenance.Create(
            clubId,
            request.EquipmentId,
            request.MaintenanceDate,
            request.Description,
            request.ResultingCondition,
            request.Cost,
            request.PerformedBy);

        _db.EquipmentMaintenanceRecords.Add(maintenance);

        // Update equipment condition based on maintenance result
        equipment.UpdateCondition(request.ResultingCondition);

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(new MaintenanceDto(
            maintenance.Id, maintenance.EquipmentId, maintenance.MaintenanceDate,
            maintenance.Description, maintenance.ResultingCondition,
            maintenance.Cost, maintenance.PerformedBy, maintenance.CreatedAt));
    }
}
