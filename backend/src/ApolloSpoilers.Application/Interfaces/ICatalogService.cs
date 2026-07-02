using ApolloSpoilers.Application.Common;
using ApolloSpoilers.Application.DTOs;

namespace ApolloSpoilers.Application.Interfaces;

public interface ICatalogService
{
    Task<PagedResult<ProductListItemDto>> SearchProductsAsync(ProductQueryDto query, CancellationToken ct = default);
    Task<Result<ProductDetailDto>> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<Result<ProductDetailDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<CategoryDto>> ListCategoriesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<string>> ListCarBrandsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<string>> ListCarModelsAsync(string carBrand, CancellationToken ct = default);
    Task<Result<ProductImageResultDto>> UpdateProductImageAsync(Guid productId, string imageUrl, CancellationToken ct);
}

public interface IProductAdminService
{
    Task<Result<ProductDetailDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<ProductDetailDto>> CreateAsync(CreateProductDto dto, CancellationToken ct = default);
    Task<Result<ProductDetailDto>> UpdateAsync(Guid id, UpdateProductDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<Result<ProductImageDto>> AddImageAsync(Guid productId, string imageUrl, bool isPrimary, CancellationToken ct = default);
    Task<Result> UpdateStockAsync(Guid productId, int quantity, int lowStockThreshold, CancellationToken ct = default);
}

public interface IReviewService
{
    Task<IReadOnlyList<ReviewDto>> ListForProductAsync(Guid productId, CancellationToken ct = default);
    Task<Result<ReviewDto>> AddReviewAsync(Guid productId, Guid userId, int rating, string? comment, CancellationToken ct = default);
    Task<Result> ApproveReviewAsync(Guid reviewId, bool approve, CancellationToken ct = default);
}
