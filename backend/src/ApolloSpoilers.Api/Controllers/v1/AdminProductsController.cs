using Asp.Versioning;
using ApolloSpoilers.Api.Common;
using ApolloSpoilers.Application.DTOs;
using ApolloSpoilers.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApolloSpoilers.Api.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/products")]
[Authorize(Roles = "Admin")]
public class AdminProductsController : ApiControllerBase
{
    private readonly IProductAdminService _admin;
    private readonly IReviewService _reviews;
    private readonly IWebHostEnvironment _env;

    private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private const long MaxImageSizeBytes = 5 * 1024 * 1024; // 5 MB

    public AdminProductsController(IProductAdminService admin, IReviewService reviews, IWebHostEnvironment env)
    {
        _admin = admin;
        _reviews = reviews;
        _env = env;
    }

    [HttpPost]
    public async Task<ActionResult<ProductDetailDto>> Create([FromBody] CreateProductDto dto, CancellationToken ct)
        => ToActionResult(await _admin.CreateAsync(dto, ct));

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProductDetailDto>> Update(Guid id, [FromBody] UpdateProductDto dto, CancellationToken ct)
        => ToActionResult(await _admin.UpdateAsync(id, dto, ct));

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
        => ToActionResult(await _admin.DeleteAsync(id, ct));

    /// <summary>Get a product by id (admin view with full details).</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDetailDto>> GetById(Guid id, CancellationToken ct)
        => ToActionResult(await _admin.GetByIdAsync(id, ct));

    [HttpPost("{productId:guid}/images")]
    public async Task<ActionResult<ProductImageDto>> AddImage(Guid productId, [FromBody] AddImageRequest req, CancellationToken ct)
        => ToActionResult(await _admin.AddImageAsync(productId, req.ImageUrl, req.IsPrimary, ct));

    /// <summary>Upload an image file for a product.</summary>
    [HttpPost("{productId:guid}/images/upload")]
    [RequestSizeLimit(MaxImageSizeBytes + 1024)]
    public async Task<ActionResult<ProductImageDto>> UploadImage(Guid productId, IFormFile file, [FromQuery] bool isPrimary = false, CancellationToken ct = default)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "No file provided.", errorCode = "VALIDATION" });

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedImageExtensions.Contains(extension))
            return BadRequest(new { error = $"Allowed image types: {string.Join(", ", AllowedImageExtensions)}", errorCode = "VALIDATION" });

        if (file.Length > MaxImageSizeBytes)
            return BadRequest(new { error = "Image must be under 5 MB.", errorCode = "VALIDATION" });

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var imagesDir = Path.Combine(_env.WebRootPath, "images", "products");
        Directory.CreateDirectory(imagesDir);

        var filePath = Path.Combine(imagesDir, fileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream, ct);
        }

        var imageUrl = $"/images/products/{fileName}";
        return ToActionResult(await _admin.AddImageAsync(productId, imageUrl, isPrimary, ct));
    }

    /// <summary>Upload an image file without attaching to a product (returns the URL for later use).</summary>
    [HttpPost("images/upload")]
    [RequestSizeLimit(MaxImageSizeBytes + 1024)]
    public async Task<ActionResult> UploadStandaloneImage(IFormFile file, CancellationToken ct = default)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "No file provided.", errorCode = "VALIDATION" });

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedImageExtensions.Contains(extension))
            return BadRequest(new { error = $"Allowed image types: {string.Join(", ", AllowedImageExtensions)}", errorCode = "VALIDATION" });

        if (file.Length > MaxImageSizeBytes)
            return BadRequest(new { error = "Image must be under 5 MB.", errorCode = "VALIDATION" });

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var imagesDir = Path.Combine(_env.WebRootPath, "images", "products");
        Directory.CreateDirectory(imagesDir);

        var filePath = Path.Combine(imagesDir, fileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream, ct);
        }

        return Ok(new { imageUrl = $"/images/products/{fileName}" });
    }

    [HttpPut("{productId:guid}/stock")]
    public async Task<ActionResult> UpdateStock(Guid productId, [FromBody] UpdateStockRequest req, CancellationToken ct)
        => ToActionResult(await _admin.UpdateStockAsync(productId, req.Quantity, req.LowStockThreshold, ct));

    /// <summary>Approve or reject a review.</summary>
    [HttpPut("reviews/{reviewId:guid}")]
    public async Task<ActionResult> ModerateReview(Guid reviewId, [FromBody] ModerateReviewRequest req, CancellationToken ct)
        => ToActionResult(await _reviews.ApproveReviewAsync(reviewId, req.Approve, ct));
}

public class AddImageRequest
{
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
}

public class UpdateStockRequest
{
    public int Quantity { get; set; }
    public int LowStockThreshold { get; set; } = 5;
}

public class ModerateReviewRequest
{
    public bool Approve { get; set; }
}
