using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Facilities.Application.Dtos;
using TheLeague.Modules.Facilities.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Facilities.Application.Queries;

public record GetFacilitiesQuery(int Page = 1, int PageSize = 20) : IRequest<PagedResult<FacilityDto>>;

public class GetFacilitiesQueryHandler : IRequestHandler<GetFacilitiesQuery, PagedResult<FacilityDto>>
{
    private readonly FacilitiesDbContext _db;

    public GetFacilitiesQueryHandler(FacilitiesDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<FacilityDto>> Handle(GetFacilitiesQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Facilities.AsNoTracking().OrderBy(f => f.Name);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(f => new FacilityDto(
                f.Id, f.Name, f.FacilityType, f.Description,
                f.Capacity, f.IsActive, f.CreatedAt, f.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<FacilityDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
