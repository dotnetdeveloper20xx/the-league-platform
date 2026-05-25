using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Programs.Application.Dtos;
using TheLeague.Modules.Programs.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Programs.Application.Queries;

public record GetProgramsQuery(
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<ProgramDto>>;

public class GetProgramsQueryHandler : IRequestHandler<GetProgramsQuery, PagedResult<ProgramDto>>
{
    private readonly ProgramsDbContext _db;

    public GetProgramsQueryHandler(ProgramsDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<ProgramDto>> Handle(GetProgramsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Programs.AsNoTracking().OrderByDescending(p => p.StartDate);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProgramDto(
                p.Id, p.Name, p.Description,
                p.ProgramType, p.SkillLevel,
                p.Capacity, p.Price,
                p.StartDate, p.EndDate,
                p.IsActive, p.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<ProgramDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
