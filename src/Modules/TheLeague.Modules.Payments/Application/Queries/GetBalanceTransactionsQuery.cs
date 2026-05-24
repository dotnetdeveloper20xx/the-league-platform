using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Payments.Application.Dtos;
using TheLeague.Modules.Payments.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Payments.Application.Queries;

public record GetBalanceTransactionsQuery(
    Guid MemberId,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<BalanceTransactionDto>>;

public class GetBalanceTransactionsQueryHandler : IRequestHandler<GetBalanceTransactionsQuery, PagedResult<BalanceTransactionDto>>
{
    private readonly PaymentsDbContext _db;

    public GetBalanceTransactionsQueryHandler(PaymentsDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<BalanceTransactionDto>> Handle(GetBalanceTransactionsQuery request, CancellationToken ct)
    {
        var query = _db.BalanceTransactions
            .AsNoTracking()
            .Where(t => t.MemberId == request.MemberId)
            .OrderByDescending(t => t.TransactionDate);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new BalanceTransactionDto(
                t.Id, t.MemberId, t.Type.ToString(), t.Amount,
                t.Description, t.ReferenceId, t.ReferenceType, t.TransactionDate))
            .ToListAsync(ct);

        return new PagedResult<BalanceTransactionDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
