using ApolloSpoilers.Domain.Common;

namespace ApolloSpoilers.Domain.Entities;

public class Wishlist : BaseEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public ICollection<WishlistItem> Items { get; set; } = new List<WishlistItem>();
}

public class WishlistItem : BaseEntity
{
    public Guid WishlistId { get; set; }
    public Wishlist? Wishlist { get; set; }

    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
}
