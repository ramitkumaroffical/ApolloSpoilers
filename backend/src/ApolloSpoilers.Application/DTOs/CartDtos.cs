namespace ApolloSpoilers.Application.DTOs;

public class CartItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductSlug { get; set; }
    public string? PrimaryImageUrl { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal => Quantity * UnitPrice;
    public int AvailableStock { get; set; }
}

public class CartDto
{
    public Guid Id { get; set; }
    public IReadOnlyList<CartItemDto> Items { get; set; } = Array.Empty<CartItemDto>();
    public decimal Subtotal => Items.Sum(i => i.LineTotal);
    public int TotalItems => Items.Sum(i => i.Quantity);
}

public class AddToCartDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class UpdateCartItemDto
{
    public int Quantity { get; set; }
}
