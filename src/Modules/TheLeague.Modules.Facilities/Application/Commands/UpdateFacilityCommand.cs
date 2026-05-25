using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Facilities.Application.Dtos;
using TheLeague.Modules.Facilities.Domain;
using TheLeague.Modules.Facilities.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Facilities.Application.Commands;

public record UpdateFacilityCommand(
    Guid Id,
    string Name,
    FacilityType FacilityType,
    string? Description,
    int? Capacity,
    bool IsActive
) : IRequest<Result<FacilityDto>>;

public class UpdateFacilityCommandHandler : IRequestHandler<UpdateFacilityCommand, Result<FacilityDto>>
{
    private readonly FacilitiesDbContext _db;

    public UpdateFacilityCommandHandler(FacilitiesDbContext db)
    {
        _db = db;
    }

    public async Task<Result<FacilityDto>> Handle(UpdateFacilityCommand request, CancellationToken cancellationToken)
    {
        var facility = await _db.Facilities.FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken);
        if (facility == null)
            return Result.Failure<FacilityDto>("Facility not found.");

        facility.Update(request.Name, request.FacilityType, request.Description, request.Capacity, request.IsActive);
        await _db.SaveChangesAsync(cancellationToken);

        var dto = new FacilityDto(
            facility.Id, facility.Name, facility.FacilityType,
            facility.Description, facility.Capacity, facility.IsActive,
            facility.CreatedAt, facility.UpdatedAt);

        return Result.Success(dto);
    }
}
