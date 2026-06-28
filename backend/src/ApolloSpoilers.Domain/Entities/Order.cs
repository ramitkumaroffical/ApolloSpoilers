using ApolloSpoilers.Domain.Common;
using ApolloSpoilers.Domain.Enums;

namespace ApolloSpoilers.Domain.Entities;

public class Order : BaseEntity
{
    /// <summary>Human-readable order reference, e.g. "APS-2025-000001".</summary>
    public string OrderNumber { get; set; } = string.Empty;

    public Guid UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public decimal Subtotal { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal TotalAmount => Subtotal + ShippingCost;

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public string ShippingFullName { get; set; } = string.Empty;
    public string ShippingAddressLine { get; set; } = string.Empty;
    public string ShippingCity { get; set; } = string.Empty;
    public string ShippingState { get; set; } = string.Empty;
    public string ShippingPostalCode { get; set; } = string.Empty;
    public string ShippingCountry { get; set; } = string.Empty;
    public string? ShippingPhone { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order? Order { get; set; }

    public Guid ProductId { get; set; }
    public Product? Product { get; set; }

    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal => Quantity * UnitPrice;
}
