namespace ApolloSpoilers.Domain.Enums;

/// <summary>Lifecycle states for an order (no payment gateway — orders begin as Pending).</summary>
public enum OrderStatus
{
    Pending = 0,
    Confirmed = 1,
    Shipped = 2,
    Delivered = 3,
    Cancelled = 4
}
