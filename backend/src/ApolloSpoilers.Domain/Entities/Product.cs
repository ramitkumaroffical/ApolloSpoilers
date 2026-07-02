using ApolloSpoilers.Domain.Common;

namespace ApolloSpoilers.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }

    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public string? Material { get; set; }
    public string? Color { get; set; }

    /// <summary>Compatible car brand (e.g. "Toyota"). Null = universal fit.</summary>
    public string? CarBrand { get; set; }
    /// <summary>Compatible car model (e.g. "Supra"). Null = universal fit.</summary>
    public string? CarModel { get; set; }
    public int? FitYearFrom { get; set; }
    public int? FitYearTo { get; set; }

    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }

    /// <summary>Denormalized aggregate of approved reviews; updated on review write.</summary>
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }

    public Inventory? Inventory { get; set; }
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
