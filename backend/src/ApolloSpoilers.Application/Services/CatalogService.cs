using AutoMapper;
using ApolloSpoilers.Application.Common;
using ApolloSpoilers.Application.DTOs;
using ApolloSpoilers.Application.Interfaces;
using ApolloSpoilers.Application.Specifications;
using ApolloSpoilers.Domain.Entities;
using ApolloSpoilers.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ApolloSpoilers.Application.Services;

public class CatalogService : ICatalogService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ILogger<CatalogService> _logger;

    public CatalogService(IUnitOfWork uow, IMapper mapper, ILogger<CatalogService> logger)
    {
        _uow = uow;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedResult<ProductListItemDto>> SearchProductsAsync(ProductQueryDto query, CancellationToken ct = default)
    {
        var productRepo = _uow.Repository<Product>();
        var spec = new ProductFilterSpecification(query);
        var items = await productRepo.ListAsync(spec, ct);
        var total = await productRepo.CountAsync(spec, ct);
        var dtos = _mapper.Map<IReadOnlyList<ProductListItemDto>>(items);
        return new PagedResult<ProductListItemDto>(dtos, total, query.Page, query.PageSize);
    }

    public async Task<Result<ProductDetailDto>> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        var productRepo = _uow.Repository<Product>();
        var spec = new ProductBySlugSpecification(slug);
        var product = (await productRepo.ListAsync(spec, ct)).FirstOrDefault();
        if (product is null)
            return Result.Failure<ProductDetailDto>($"Product '{slug}' not found.", "NOT_FOUND");

        return Result.Success(_mapper.Map<ProductDetailDto>(product));
    }

    public async Task<Result<ProductDetailDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var spec = new ProductByIdSpecification(id);
        var product = (await _uow.Repository<Product>().ListAsync(spec, ct)).FirstOrDefault();
        if (product is null)
            return Result.Failure<ProductDetailDto>("Product not found.", "NOT_FOUND");
        return Result.Success(_mapper.Map<ProductDetailDto>(product));
    }

    public async Task<IReadOnlyList<CategoryDto>> ListCategoriesAsync(CancellationToken ct = default)
    {
        var categories = await _uow.Repository<Category>().ListAsync(ct: ct);
        return _mapper.Map<IReadOnlyList<CategoryDto>>(categories);
    }

    public async Task<IReadOnlyList<string>> ListCarBrandsAsync(CancellationToken ct = default)
    {
        var productRepo = _uow.Repository<Product>();
        var activeSpec = new ActiveProductBrandsSpecification();
        var products = await productRepo.ListAsync(activeSpec, ct);
        return products.Select(p => p.CarBrand!)
            .Where(b => !string.IsNullOrWhiteSpace(b))
            .Distinct()
            .OrderBy(b => b)
            .ToList();
    }

    public async Task<IReadOnlyList<string>> ListCarModelsAsync(string carBrand, CancellationToken ct = default)
    {
        var spec = new CarModelsSpecification(carBrand);
        var products = await _uow.Repository<Product>().ListAsync(spec, ct);
        return products.Select(p => p.CarModel!)
            .Where(m => !string.IsNullOrWhiteSpace(m))
            .Distinct()
            .OrderBy(m => m)
            .ToList();
    }
}
