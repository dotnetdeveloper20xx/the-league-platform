using TheLeague.Shared.Domain.Entities;

namespace TheLeague.Modules.Shop.Domain;

public class Order : TenantEntity
{
    public Guid MemberId { get; private set; }
    public string OrderReference { get; private set; } = string.Empty;
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;
    public decimal TotalAmount { get; private set; }
    public DateTime OrderedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? ConfirmedAt { get; private set; }
    public DateTime? DispatchedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public DateTime? RefundedAt { get; private set; }

    // Navigation
    public ICollection<OrderItem> Items { get; private set; } = new List<OrderItem>();

    private static readonly Dictionary<OrderStatus, HashSet<OrderStatus>> _validTransitions = new()
    {
        { OrderStatus.Pending, new HashSet<OrderStatus> { OrderStatus.Confirmed, OrderStatus.Refunded } },
        { OrderStatus.Confirmed, new HashSet<OrderStatus> { OrderStatus.Dispatched, OrderStatus.Refunded } },
        { OrderStatus.Dispatched, new HashSet<OrderStatus> { OrderStatus.Delivered, OrderStatus.Refunded } },
        { OrderStatus.Delivered, new HashSet<OrderStatus>() },
        { OrderStatus.Refunded, new HashSet<OrderStatus>() }
    };

    private Order() { }

    public static Order Create(Guid clubId, Guid memberId, string orderReference, decimal totalAmount)
    {
        return new Order
        {
            ClubId = clubId,
            MemberId = memberId,
            OrderReference = orderReference,
            TotalAmount = totalAmount,
            Status = OrderStatus.Pending,
            OrderedAt = DateTime.UtcNow
        };
    }

    public bool CanTransitionTo(OrderStatus newStatus)
    {
        return _validTransitions.TryGetValue(Status, out var validTargets) && validTargets.Contains(newStatus);
    }

    public void TransitionTo(OrderStatus newStatus)
    {
        if (!CanTransitionTo(newStatus))
            throw new InvalidOperationException(
                $"Cannot transition order from {Status} to {newStatus}");

        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;

        switch (newStatus)
        {
            case OrderStatus.Confirmed:
                ConfirmedAt = DateTime.UtcNow;
                break;
            case OrderStatus.Dispatched:
                DispatchedAt = DateTime.UtcNow;
                break;
            case OrderStatus.Delivered:
                DeliveredAt = DateTime.UtcNow;
                break;
            case OrderStatus.Refunded:
                RefundedAt = DateTime.UtcNow;
                break;
        }
    }
}
