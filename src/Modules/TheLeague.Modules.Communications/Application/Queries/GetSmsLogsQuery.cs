using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Communications.Application.Dtos;
using TheLeague.Modules.Communications.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Communications.Application.Queries;

public record GetSmsLogsQuery(int Page = 1, int PageSize = 20) : IRequest<PagedResult<SmsLogDto>>;

public class GetSmsLogsQueryHandler : IRequestHandler<GetSmsLogsQuery, PagedResult<SmsLogDto>>
{
    private readonly CommunicationsDbContext _db;

    public GetSmsLogsQueryHandler(CommunicationsDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<SmsLogDto>> Handle(GetSmsLogsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.SmsLogs.AsNoTracking().OrderByDescending(s => s.SentAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new SmsLogDto(
                s.Id, s.RecipientMemberId, s.RecipientPhone, s.Message,
                s.Status, s.SentAt, s.FailureReason
            ))
            .ToListAsync(cancellationToken);

        return new PagedResult<SmsLogDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
