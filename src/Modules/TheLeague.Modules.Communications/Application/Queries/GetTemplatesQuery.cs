using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Communications.Application.Dtos;
using TheLeague.Modules.Communications.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Communications.Application.Queries;

public record GetTemplatesQuery(int Page = 1, int PageSize = 20) : IRequest<PagedResult<TemplateDto>>;

public class GetTemplatesQueryHandler : IRequestHandler<GetTemplatesQuery, PagedResult<TemplateDto>>
{
    private readonly CommunicationsDbContext _db;

    public GetTemplatesQueryHandler(CommunicationsDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<TemplateDto>> Handle(GetTemplatesQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Templates.AsNoTracking().OrderBy(t => t.Name);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new TemplateDto(
                t.Id, t.Name, t.TemplateType, t.Subject, t.Body, t.IsActive, t.CreatedAt, t.UpdatedAt
            ))
            .ToListAsync(cancellationToken);

        return new PagedResult<TemplateDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
