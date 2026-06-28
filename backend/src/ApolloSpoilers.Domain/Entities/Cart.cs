using ApolloSpoilers.Domain.Common;

namespace ApolloSpoilers.Domain.Entities;

public class Cart : BaseEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();

    public decimal Subtotal => Items.Sum(i => i.UnitPrice * i.Quantity);
    public int TotalItems => Items.Sum(i => i.Quantity);
}

public class CartItem : BaseEntity
{
    public Guid CartId { get; set; }
    public Cart? Cart { get; set; }

    public Guid ProductId { get; set; }
    public Product? Product { get; set; }

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
