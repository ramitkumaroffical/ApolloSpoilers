namespace ApolloSpoilers.Application.DTOs;

public class ProductListItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public string? CarBrand { get; set; }
    public string? CarModel { get; set; }
    public string? PrimaryImageUrl { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public int StockQuantity { get; set; }
    public bool IsFeatured { get; set; }
    public string? CategoryName { get; set; }
}
