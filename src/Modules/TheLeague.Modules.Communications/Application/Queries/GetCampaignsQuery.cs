using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Communications.Application.Dtos;
using TheLeague.Modules.Communications.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Communications.Application.Queries;

public record GetCampaignsQuery(int Page = 1, int PageSize = 20) : IRequest<PagedResult<BulkEmailCampaignDto>>;

public class GetCampaignsQueryHandler : IRequestHandler<GetCampaignsQuery, PagedResult<BulkEmailCampaignDto>>
{
    private readonly CommunicationsDbContext _db;

    public GetCampaignsQueryHandler(CommunicationsDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<BulkEmailCampaignDto>> Handle(GetCampaignsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.BulkEmailCampaigns.AsNoTracking().OrderByDescending(c => c.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new BulkEmailCampaignDto(
                c.Id, c.Name, c.Subject, c.Body, c.TargetSegment,
                c.TotalRecipients, c.SentCount, c.FailedCount, c.ExcludedCount,
                c.Status, c.CreatedAt, c.CompletedAt
            ))
            .ToListAsync(cancellationToken);

        return new PagedResult<BulkEmailCampaignDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
