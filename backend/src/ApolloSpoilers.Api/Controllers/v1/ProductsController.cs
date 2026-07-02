using ApolloSpoilers.Api.Common;
using ApolloSpoilers.Application.Common;
using ApolloSpoilers.Application.DTOs;
using ApolloSpoilers.Application.Interfaces;
using ApolloSpoilers.Domain.Interfaces;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApolloSpoilers.Api.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/products")]
public class ProductsController : ApiControllerBase
{
    private readonly ICatalogService _catalog;
    private readonly IReviewService _reviews;
    private readonly IImageStorageService _imageStorage;

    public ProductsController(ICatalogService catalog, IReviewService reviews, IImageStorageService imageStorage)
    {
        _catalog = catalog;
        _reviews = reviews;
        _imageStorage = imageStorage;
    }

    /// <summary>List/search/filter products. Returns paged result.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ProductListItemDto>>> Search([FromQuery] ProductQueryDto query, CancellationToken ct)
        => Ok(await _catalog.SearchProductsAsync(query, ct));

    /// <summary>Get a product by slug.</summary>
    [HttpGet("{slug}")]
    public async Task<ActionResult<ProductDetailDto>> GetBySlug(string slug, CancellationToken ct)
        => ToActionResult(await _catalog.GetBySlugAsync(slug, ct));

    /// <summary>List all categories.</summary>
    [HttpGet("categories")]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> Categories(CancellationToken ct)
        => Ok(await _catalog.ListCategoriesAsync(ct));

    /// <summary>List distinct car brands (for filter dropdowns).</summary>
    [HttpGet("car-brands")]
    public async Task<ActionResult<IReadOnlyList<string>>> CarBrands(CancellationToken ct)
        => Ok(await _catalog.ListCarBrandsAsync(ct));

    /// <summary>List distinct car models for a given brand.</summary>
    [HttpGet("car-models/{carBrand}")]
    public async Task<ActionResult<IReadOnlyList<string>>> CarModels(string carBrand, CancellationToken ct)
        => Ok(await _catalog.ListCarModelsAsync(carBrand, ct));

    /// <summary>List approved reviews for a product.</summary>
    [HttpGet("{productId:guid}/reviews")]
    public async Task<ActionResult<IReadOnlyList<ReviewDto>>> Reviews(Guid productId, CancellationToken ct)
        => Ok(await _reviews.ListForProductAsync(productId, ct));

    /// <summary>Submit a review (requires auth).</summary>
    [Authorize]
    [HttpPost("{productId:guid}/reviews")]
    public async Task<ActionResult<ReviewDto>> AddReview(Guid productId, [FromBody] AddReviewRequest dto, CancellationToken ct)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return Unauthorized();
        return ToActionResult(await _reviews.AddReviewAsync(productId, Guid.Parse(userId), dto.Rating, dto.Comment, ct));
    }

    /// <summary>Upload/replace a product image (admin only).</summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("{productId:guid}/image")]
    [ProducesResponseType(typeof(ProductImageResultDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProductImageResultDto>> UploadImage(Guid productId, IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest("No file provided.");

        await using var stream = file.OpenReadStream();
        var imageUrl = await _imageStorage.UploadImageAsync(stream, file.FileName);

        return ToActionResult(await _catalog.UpdateProductImageAsync(productId, imageUrl, ct));
    }
}

public class AddReviewRequest
{
    public int Rating { get; set; }
    public string? Comment { get; set; }
}