using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Payments.Application.Dtos;
using TheLeague.Modules.Payments.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Enums;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Payments.Application.Queries;

public record GetPaymentsQuery(
    int Page = 1,
    int PageSize = 20,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    PaymentStatus? Status = null,
    PaymentMethod? Method = null
) : IRequest<PagedResult<PaymentDto>>;

public class GetPaymentsQueryHandler : IRequestHandler<GetPaymentsQuery, PagedResult<PaymentDto>>
{
    private readonly PaymentsDbContext _db;

    public GetPaymentsQueryHandler(PaymentsDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<PaymentDto>> Handle(GetPaymentsQuery request, CancellationToken ct)
    {
        var query = _db.Payments.AsNoTracking().AsQueryable();

        if (request.FromDate.HasValue)
            query = query.Where(p => p.PaymentDate >= request.FromDate.Value);

        if (request.ToDate.HasValue)
            query = query.Where(p => p.PaymentDate <= request.ToDate.Value);

        if (request.Status.HasValue)
            query = query.Where(p => p.Status == request.Status.Value);

        if (request.Method.HasValue)
            query = query.Where(p => p.Method == request.Method.Value);

        query = query.OrderByDescending(p => p.PaymentDate);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new PaymentDto(
                p.Id, p.MemberId, p.Amount, p.Method, p.Status, p.Type,
                p.ExternalTransactionId, p.FailureReason, p.PaymentDate,
                p.PlatformFee, p.Description))
            .ToListAsync(ct);

        return new PagedResult<PaymentDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
