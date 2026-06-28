using AutoMapper;
using ApolloSpoilers.Application.Common;
using ApolloSpoilers.Application.DTOs;
using ApolloSpoilers.Application.Interfaces;
using ApolloSpoilers.Application.Specifications;
using ApolloSpoilers.Domain.Entities;
using ApolloSpoilers.Domain.Exceptions;
using ApolloSpoilers.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ApolloSpoilers.Application.Services;

public class ProductAdminService : IProductAdminService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductAdminService> _logger;

    public ProductAdminService(IUnitOfWork uow, IMapper mapper, ILogger<ProductAdminService> logger)
    {
        _uow = uow;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<ProductDetailDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var productRepo = _uow.Repository<Product>();
        var spec = new ProductByIdSpecification(id);
        var products = await productRepo.ListAsync(spec, ct);
        var product = products.FirstOrDefault();
        if (product is null)
            return Result.Failure<ProductDetailDto>("Product not found.", "NOT_FOUND");

        return Result.Success(_mapper.Map<ProductDetailDto>(product));
    }

    public async Task<Result<ProductDetailDto>> CreateAsync(CreateProductDto dto, CancellationToken ct = default)
    {
        var productRepo = _uow.Repository<Product>();

        // Check slug uniqueness
        var slug = GenerateSlug(dto.Name);
        if (await productRepo.AnyAsync(p => p.Slug == slug, ct))
            slug = $"{slug}-{Guid.NewGuid():N}".Substring(0, Math.Min(slug.Length + 8, 200));

        var product = new Product
        {
            Name = dto.Name,
            Slug = slug,
            Description = dto.Description,
            Price = dto.Price,
            CompareAtPrice = dto.CompareAtPrice,
            Material = dto.Material,
            Color = dto.Color,
            CarBrand = dto.CarBrand,
            CarModel = dto.CarModel,
            FitYearFrom = dto.FitYearFrom,
            FitYearTo = dto.FitYearTo,
            CategoryId = dto.CategoryId,
            IsActive = dto.IsActive,
            IsFeatured = dto.IsFeatured,
            Inventory = new Inventory
            {
                StockQuantity = dto.InitialStock,
                LowStockThreshold = dto.LowStockThreshold,
                LastStockUpdate = DateTime.UtcNow
            }
        };

        await productRepo.AddAsync(product, ct);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Product '{Name}' created (Id={Id})", product.Name, product.Id);

        var spec = new ProductByIdSpecification(product.Id);
        var created = (await productRepo.ListAsync(spec, ct)).First();
        return Result.Success(_mapper.Map<ProductDetailDto>(created));
    }

    public async Task<Result<ProductDetailDto>> UpdateAsync(Guid id, UpdateProductDto dto, CancellationToken ct = default)
    {
        var productRepo = _uow.Repository<Product>();
        var existing = await productRepo.GetByIdAsync(id, ct);
        if (existing is null)
            return Result.Failure<ProductDetailDto>("Product not found.", "NOT_FOUND");

        existing.Name = dto.Name;
        existing.Slug = GenerateSlug(dto.Name);
        existing.Description = dto.Description;
        existing.Price = dto.Price;
        existing.CompareAtPrice = dto.CompareAtPrice;
        existing.Material = dto.Material;
        existing.Color = dto.Color;
        existing.CarBrand = dto.CarBrand;
        existing.CarModel = dto.CarModel;
        existing.FitYearFrom = dto.FitYearFrom;
        existing.FitYearTo = dto.FitYearTo;
        existing.CategoryId = dto.CategoryId;
        existing.IsActive = dto.IsActive;
        existing.IsFeatured = dto.IsFeatured;
        existing.UpdatedAt = DateTime.UtcNow;

        if (existing.Inventory != null)
        {
            existing.Inventory.LowStockThreshold = dto.LowStockThreshold;
            existing.Inventory.LastStockUpdate = DateTime.UtcNow;
        }

        productRepo.Update(existing);
        await _uow.SaveChangesAsync(ct);

        var spec = new ProductByIdSpecification(id);
        var updated = (await productRepo.ListAsync(spec, ct)).First();
        return Result.Success(_mapper.Map<ProductDetailDto>(updated));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var productRepo = _uow.Repository<Product>();
        var existing = await productRepo.GetByIdAsync(id, ct);
        if (existing is null)
            return Result.Failure("Product not found.", "NOT_FOUND");

        existing.IsActive = false;
        existing.UpdatedAt = DateTime.UtcNow;
        productRepo.Update(existing);
        await _uow.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task<Result<ProductImageDto>> AddImageAsync(Guid productId, string imageUrl, bool isPrimary, CancellationToken ct = default)
    {
        var productRepo = _uow.Repository<Product>();
        var imageRepo = _uow.Repository<ProductImage>();
        var product = await productRepo.GetByIdAsync(productId, ct);
        if (product is null)
            return Result.Failure<ProductImageDto>("Product not found.", "NOT_FOUND");

        if (isPrimary)
        {
            // Demote existing primary
            var existing = (await productRepo.ListAsync(new ProductByIdSpecification(productId), ct)).First();
            foreach (var img in existing.Images.Where(i => i.IsPrimary))
            {
                img.IsPrimary = false;
                imageRepo.Update(img);
            }
        }

        var order = product.Images.Any() ? product.Images.Max(i => i.DisplayOrder) + 1 : 1;
        var image = new ProductImage
        {
            ProductId = productId,
            ImageUrl = imageUrl,
            IsPrimary = isPrimary,
            DisplayOrder = order
        };
        await imageRepo.AddAsync(image, ct);
        await _uow.SaveChangesAsync(ct);

        return Result.Success(_mapper.Map<ProductImageDto>(image));
    }

    public async Task<Result> UpdateStockAsync(Guid productId, int quantity, int lowStockThreshold, CancellationToken ct = default)
    {
        var productRepo = _uow.Repository<Product>();
        var product = await productRepo.GetByIdAsync(productId, ct);
        if (product?.Inventory is null)
            return Result.Failure("Product or inventory not found.", "NOT_FOUND");

        product.Inventory.StockQuantity = quantity;
        product.Inventory.LowStockThreshold = lowStockThreshold;
        product.Inventory.LastStockUpdate = DateTime.UtcNow;
        _uow.Repository<Inventory>().Update(product.Inventory);
        await _uow.SaveChangesAsync(ct);

        return Result.Success();
    }

    private static string GenerateSlug(string name) =>
        name.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("--", "-")
            .Trim('-');
}
