namespace ApolloSpoilers.Domain.Interfaces.Ai;

/// <summary>A single retrieved document from the vector store.</summary>
public record VectorSearchHit(
    Guid PointId,
    double Score,
    string Text,
    IReadOnlyDictionary<string, object?> Payload);

/// <summary>Abstraction over the Qdrant vector DB used by the Aasra RAG pipeline.</summary>
public interface IVectorStore
{
    /// <summary>Ensure the configured collection exists with the right vector size + distance.</summary>
    Task EnsureCollectionAsync(int vectorSize, CancellationToken ct = default);

    /// <summary>Upsert a single point with payload + vector.</summary>
    Task UpsertAsync(Guid pointId, ReadOnlyMemory<float> vector, IReadOnlyDictionary<string, object?> payload, CancellationToken ct = default);

    /// <summary>Batch upsert. Implementation may chunk internally.</summary>
    Task UpsertBatchAsync(IEnumerable<(Guid PointId, ReadOnlyMemory<float> Vector, IReadOnlyDictionary<string, object?> Payload)> points, CancellationToken ct = default);

    /// <summary>Delete a single point by id.</summary>
    Task DeleteAsync(Guid pointId, CancellationToken ct = default);

    /// <summary>Delete many points by id.</summary>
    Task DeleteBatchAsync(IEnumerable<Guid> pointIds, CancellationToken ct = default);

    /// <summary>Approximate nearest-neighbour search. Returns top-K hits.</summary>
    Task<IReadOnlyList<VectorSearchHit>> SearchAsync(ReadOnlyMemory<float> queryVector, int topK, CancellationToken ct = default);
}
