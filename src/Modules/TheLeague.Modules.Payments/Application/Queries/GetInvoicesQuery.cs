using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Payments.Application.Dtos;
using TheLeague.Modules.Payments.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Enums;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Payments.Application.Queries;

public record GetInvoicesQuery(
    int Page = 1,
    int PageSize = 20,
    InvoiceStatus? Status = null
) : IRequest<PagedResult<InvoiceDto>>;

public class GetInvoicesQueryHandler : IRequestHandler<GetInvoicesQuery, PagedResult<InvoiceDto>>
{
    private readonly PaymentsDbContext _db;

    public GetInvoicesQueryHandler(PaymentsDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<InvoiceDto>> Handle(GetInvoicesQuery request, CancellationToken ct)
    {
        var query = _db.Invoices
            .AsNoTracking()
            .Include(i => i.LineItems)
            .AsQueryable();

        if (request.Status.HasValue)
            query = query.Where(i => i.Status == request.Status.Value);

        query = query.OrderByDescending(i => i.IssueDate);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(i => new InvoiceDto(
                i.Id, i.MemberId, i.InvoiceNumber, i.Status,
                i.IssueDate, i.DueDate, i.TotalAmount, i.PaidAmount, i.Notes,
                i.LineItems.Select(li => new InvoiceLineItemDto(
                    li.Id, li.Description, li.Quantity, li.UnitPrice, li.TotalPrice, li.FeeType
                )).ToList()))
            .ToListAsync(ct);

        return new PagedResult<InvoiceDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
