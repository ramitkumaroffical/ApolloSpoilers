namespace ApolloSpoilers.Application.DTOs;

public class ProductDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public string? Material { get; set; }
    public string? Color { get; set; }
    public string? CarBrand { get; set; }
    public string? CarModel { get; set; }
    public int? FitYearFrom { get; set; }
    public int? FitYearTo { get; set; }

    public Guid CategoryId { get; set; }
    public string? CategoryName { get; set; }

    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }

    public int StockQuantity { get; set; }
    public int LowStockThreshold { get; set; }

    public IReadOnlyList<ProductImageDto> Images { get; set; } = Array.Empty<ProductImageDto>();
    public IReadOnlyList<ReviewDto> RecentReviews { get; set; } = Array.Empty<ReviewDto>();
}
