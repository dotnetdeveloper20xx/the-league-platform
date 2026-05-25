using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Facilities.Application.Dtos;
using TheLeague.Modules.Facilities.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Facilities.Application.Queries;

public record GetFacilityByIdQuery(Guid Id) : IRequest<Result<FacilityDto>>;

public class GetFacilityByIdQueryHandler : IRequestHandler<GetFacilityByIdQuery, Result<FacilityDto>>
{
    private readonly FacilitiesDbContext _db;

    public GetFacilityByIdQueryHandler(FacilitiesDbContext db)
    {
        _db = db;
    }

    public async Task<Result<FacilityDto>> Handle(GetFacilityByIdQuery request, CancellationToken cancellationToken)
    {
        var facility = await _db.Facilities
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken);

        if (facility == null)
            return Result.Failure<FacilityDto>("Facility not found.");

        var dto = new FacilityDto(
            facility.Id, facility.Name, facility.FacilityType,
            facility.Description, facility.Capacity, facility.IsActive,
            facility.CreatedAt, facility.UpdatedAt);

        return Result.Success(dto);
    }
}
