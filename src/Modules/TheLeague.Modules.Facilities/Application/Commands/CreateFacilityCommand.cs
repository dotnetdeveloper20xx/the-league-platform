using MediatR;
using TheLeague.Modules.Facilities.Application.Dtos;
using TheLeague.Modules.Facilities.Domain;
using TheLeague.Modules.Facilities.Infrastructure.Persistence;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Facilities.Application.Commands;

public record CreateFacilityCommand(
    string Name,
    FacilityType FacilityType,
    string? Description,
    int? Capacity
) : IRequest<Result<FacilityDto>>;

public class CreateFacilityCommandHandler : IRequestHandler<CreateFacilityCommand, Result<FacilityDto>>
{
    private readonly FacilitiesDbContext _db;
    private readonly ITenantService _tenantService;

    public CreateFacilityCommandHandler(FacilitiesDbContext db, ITenantService tenantService)
    {
        _db = db;
        _tenantService = tenantService;
    }

    public async Task<Result<FacilityDto>> Handle(CreateFacilityCommand request, CancellationToken cancellationToken)
    {
        var clubId = _tenantService.CurrentTenantId;
        if (clubId == null)
            return Result.Failure<FacilityDto>("Tenant context is required.");

        var facility = Facility.Create(clubId.Value, request.Name, request.FacilityType, request.Description, request.Capacity);

        _db.Facilities.Add(facility);
        await _db.SaveChangesAsync(cancellationToken);

        var dto = new FacilityDto(
            facility.Id, facility.Name, facility.FacilityType,
            facility.Description, facility.Capacity, facility.IsActive,
            facility.CreatedAt, facility.UpdatedAt);

        return Result.Success(dto);
    }
}
