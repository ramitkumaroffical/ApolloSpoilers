namespace ApolloSpoilers.Application.DTOs;

public class CreateProductDto
{
    public string Name { get; set; } = string.Empty;
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
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }

    public int InitialStock { get; set; }
    public int LowStockThreshold { get; set; } = 5;
}

public class UpdateProductDto : CreateProductDto { }
