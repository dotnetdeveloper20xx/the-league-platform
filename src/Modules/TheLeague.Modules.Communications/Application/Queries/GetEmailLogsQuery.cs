using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Communications.Application.Dtos;
using TheLeague.Modules.Communications.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Communications.Application.Queries;

public record GetEmailLogsQuery(int Page = 1, int PageSize = 20) : IRequest<PagedResult<EmailLogDto>>;

public class GetEmailLogsQueryHandler : IRequestHandler<GetEmailLogsQuery, PagedResult<EmailLogDto>>
{
    private readonly CommunicationsDbContext _db;

    public GetEmailLogsQueryHandler(CommunicationsDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<EmailLogDto>> Handle(GetEmailLogsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.EmailLogs.AsNoTracking().OrderByDescending(e => e.SentAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(e => new EmailLogDto(
                e.Id, e.RecipientMemberId, e.RecipientEmail, e.TemplateType,
                e.Subject, e.Status, e.SentAt, e.DeliveredAt, e.FailureReason
            ))
            .ToListAsync(cancellationToken);

        return new PagedResult<EmailLogDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
