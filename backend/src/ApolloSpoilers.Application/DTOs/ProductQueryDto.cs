namespace ApolloSpoilers.Application.DTOs;

public enum ProductSortOption
{
    Newest,
    PriceAscending,
    PriceDescending,
    Rating,
    NameAscending
}

/// <summary>Filter + pagination input for product listing.</summary>
public class ProductQueryDto
{
    public string? Search { get; set; }
    public Guid? CategoryId { get; set; }
    public string? CarBrand { get; set; }
    public string? CarModel { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? IsFeatured { get; set; }
    public ProductSortOption SortBy { get; set; } = ProductSortOption.Newest;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
