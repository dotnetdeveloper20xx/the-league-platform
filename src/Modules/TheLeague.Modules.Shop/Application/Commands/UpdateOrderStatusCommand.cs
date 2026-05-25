using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Shop.Application.Dtos;
using TheLeague.Modules.Shop.Domain;
using TheLeague.Modules.Shop.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Shop.Application.Commands;

public record UpdateOrderStatusCommand(
    Guid OrderId,
    OrderStatus NewStatus
) : IRequest<Result<OrderDto>>;

public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, Result<OrderDto>>
{
    private readonly ShopDbContext _db;

    public UpdateOrderStatusCommandHandler(ShopDbContext db)
    {
        _db = db;
    }

    public async Task<Result<OrderDto>> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null)
            return Result.Failure<OrderDto>("Order not found");

        if (!order.CanTransitionTo(request.NewStatus))
            return Result.Failure<OrderDto>(
                $"Cannot transition order from {order.Status} to {request.NewStatus}");

        order.TransitionTo(request.NewStatus);
        await _db.SaveChangesAsync(cancellationToken);

        // TODO: Send confirmation email on Confirmed status

        return Result.Success(MapToDto(order));
    }

    private static OrderDto MapToDto(Order order) => new(
        order.Id, order.MemberId, order.OrderReference, order.Status,
        order.TotalAmount, order.OrderedAt, order.ConfirmedAt,
        order.DispatchedAt, order.DeliveredAt, order.RefundedAt,
        order.Items.Select(i => new OrderItemDto(
            i.Id, i.ProductId, i.VariantId, i.ProductName,
            i.VariantDescription, i.Quantity, i.UnitPrice, i.TotalPrice)).ToList());
}
