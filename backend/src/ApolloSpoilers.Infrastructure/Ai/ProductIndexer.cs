using System.Text.Json;
using ApolloSpoilers.Application.Interfaces;
using ApolloSpoilers.Application.Specifications;
using ApolloSpoilers.Domain.Entities;
using ApolloSpoilers.Domain.Enums;
using ApolloSpoilers.Domain.Interfaces;
using ApolloSpoilers.Domain.Interfaces.Ai;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ApolloSpoilers.Infrastructure.Ai;

/// <summary>
/// Keeps the Qdrant knowledge base in sync with the product catalog. Each product
/// is embedded as a chunk and stored; AiKnowledgeChunk rows in SQL map back to the
/// Qdrant point id so deletes can be cleaned up.
/// </summary>
public class ProductIndexer : IProductIndexer
{
    private readonly IUnitOfWork _uow;
    private readonly IEmbeddingService _embedder;
    private readonly IVectorStore _vectorStore;
    private readonly IAiKnowledgeChunkRepository _chunkRepo;
    private readonly ILogger<ProductIndexer> _logger;

    public ProductIndexer(
        IUnitOfWork uow,
        IEmbeddingService embedder,
        IVectorStore vectorStore,
        IAiKnowledgeChunkRepository chunkRepo,
        ILogger<ProductIndexer> logger)
    {
        _uow = uow;
        _embedder = embedder;
        _vectorStore = vectorStore;
        _chunkRepo = chunkRepo;
        _logger = logger;
    }

    public async Task IndexProductAsync(Guid productId, CancellationToken ct = default)
    {
        var product = (await _uow.Repository<Product>().ListAsync(new ProductByIdSpecification(productId), ct)).FirstOrDefault();
        if (product is null)
        {
            _logger.LogWarning("IndexProduct: product {Id} not found", productId);
            return;
        }

        await _vectorStore.EnsureCollectionAsync(_embedder.VectorSize, ct);

        var chunkText = ProductIndexerTextBuilder.BuildChunkText(product);
        var vector = await _embedder.EmbedAsync(chunkText, ct);

        var pointId = Guid.NewGuid();
        var payload = new Dictionary<string, object?>
        {
            ["text"] = chunkText,
            ["productId"] = product.Id.ToString(),
            ["productSlug"] = product.Slug,
            ["productName"] = product.Name,
            ["price"] = (double)product.Price,
            ["carBrand"] = product.CarBrand,
            ["carModel"] = product.CarModel,
            ["category"] = product.Category?.Name,
            ["material"] = product.Material,
            ["color"] = product.Color,
            ["sourceType"] = KnowledgeSourceType.Product.ToString()
        };

        // Remove existing chunks for this product, then re-add
        await RemoveFromIndexAsync(productId, ct);

        await _vectorStore.UpsertAsync(pointId, vector, payload, ct);
        await _chunkRepo.AddAsync(new AiKnowledgeChunk
        {
            ProductId = productId,
            SourceType = KnowledgeSourceType.Product,
            ChunkText = chunkText,
            QdrantPointId = pointId,
            Metadata = JsonSerializer.Serialize(payload)
        }, ct);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Indexed product {ProductId} as point {PointId}", productId, pointId);
    }

    public async Task RemoveFromIndexAsync(Guid productId, CancellationToken ct = default)
    {
        var chunks = await _chunkRepo.GetByProductIdAsync(productId, ct);
        if (chunks.Count == 0) return;

        await _vectorStore.DeleteBatchAsync(chunks.Select(c => c.QdrantPointId), ct);
        _chunkRepo.RemoveRange(chunks);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task ReindexAllAsync(CancellationToken ct = default)
    {
        var all = await _uow.Repository<Product>().ListAsync(ct: ct);

        foreach (var product in all.Where(p => p.IsActive))
        {
            try
            {
                await IndexProductAsync(product.Id, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to index product {Id}", product.Id);
            }
        }
        _logger.LogInformation("Reindex complete. Processed {Count} products.", all.Count);
    }
}

internal static class ProductIndexerTextBuilder
{
    public static string BuildChunkText(Product p)
    {
        var fit = (p.CarBrand, p.CarModel) switch
        {
            (null, _) => "Universal fit (works on most vehicles)",
            (_, null) => $"Fits {p.CarBrand}",
            _ => p.FitYearFrom.HasValue
                ? $"Fits {p.CarBrand} {p.CarModel} ({p.FitYearFrom}{(p.FitYearTo.HasValue ? "-" + p.FitYearTo : "+")})"
                : $"Fits {p.CarBrand} {p.CarModel}"
        };

        return $"""
        Product: {p.Name}
        Category: {p.Category?.Name ?? "Uncategorized"}
        Price: ${p.Price:F2}
        {fit}
        Material: {p.Material ?? "n/a"}
        Color/Finish: {p.Color ?? "n/a"}
        Description: {p.Description}
        """;
    }
}
