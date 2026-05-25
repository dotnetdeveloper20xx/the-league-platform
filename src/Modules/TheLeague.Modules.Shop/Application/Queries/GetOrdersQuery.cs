using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Shop.Application.Dtos;
using TheLeague.Modules.Shop.Domain;
using TheLeague.Modules.Shop.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Shop.Application.Queries;

public record GetOrdersQuery(
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<OrderDto>>;

public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, PagedResult<OrderDto>>
{
    private readonly ShopDbContext _db;

    public GetOrdersQueryHandler(ShopDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<OrderDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var pageSize = Math.Min(request.PageSize, 100);
        if (pageSize < 1) pageSize = 20;

        var query = _db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .OrderByDescending(o => o.OrderedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.Page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(MapToDto).ToList();

        return new PagedResult<OrderDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = pageSize
        };
    }

    private static OrderDto MapToDto(Order order) => new(
        order.Id, order.MemberId, order.OrderReference, order.Status,
        order.TotalAmount, order.OrderedAt, order.ConfirmedAt,
        order.DispatchedAt, order.DeliveredAt, order.RefundedAt,
        order.Items.Select(i => new OrderItemDto(
            i.Id, i.ProductId, i.VariantId, i.ProductName,
            i.VariantDescription, i.Quantity, i.UnitPrice, i.TotalPrice)).ToList());
}
