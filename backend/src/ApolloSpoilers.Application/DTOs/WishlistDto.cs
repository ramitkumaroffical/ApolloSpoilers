namespace ApolloSpoilers.Application.DTOs;

public class WishlistItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductSlug { get; set; }
    public decimal Price { get; set; }
    public string? PrimaryImageUrl { get; set; }
}

public class WishlistDto
{
    public Guid Id { get; set; }
    public IReadOnlyList<WishlistItemDto> Items { get; set; } = Array.Empty<WishlistItemDto>();
}
