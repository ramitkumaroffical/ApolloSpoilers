namespace ApolloSpoilers.Domain.Interfaces.Ai;

/// <summary>Abstraction over an embedding-generation backend (OpenAI-compatible).</summary>
public interface IEmbeddingService
{
    /// <summary>Embeds a single text input into a float vector.</summary>
    Task<ReadOnlyMemory<float>> EmbedAsync(string text, CancellationToken ct = default);

    /// <summary>Embeds multiple texts in a single batch call.</summary>
    Task<IReadOnlyList<ReadOnlyMemory<float>>> EmbedBatchAsync(IEnumerable<string> texts, CancellationToken ct = default);

    /// <summary>Dimensionality of vectors this service produces.</summary>
    int VectorSize { get; }
}
