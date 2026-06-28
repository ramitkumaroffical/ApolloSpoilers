using ApolloSpoilers.Domain.Common;

namespace ApolloSpoilers.Domain.Entities;

/// <summary>One-to-one stock record for a product.</summary>
public class Inventory : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }

    public int StockQuantity { get; set; }
    public int LowStockThreshold { get; set; } = 5;

    public DateTime? LastStockUpdate { get; set; }
}
