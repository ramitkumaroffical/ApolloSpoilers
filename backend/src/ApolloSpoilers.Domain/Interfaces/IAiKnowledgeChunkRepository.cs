using ApolloSpoilers.Domain.Entities;

namespace ApolloSpoilers.Domain.Interfaces;

/// <summary>Specialized repository for AI knowledge chunk records.</summary>
public interface IAiKnowledgeChunkRepository
{
    Task<AiKnowledgeChunk?> GetByQdrantPointIdAsync(Guid pointId, CancellationToken ct = default);
    Task<IReadOnlyList<AiKnowledgeChunk>> GetByProductIdAsync(Guid productId, CancellationToken ct = default);
    Task AddAsync(AiKnowledgeChunk chunk, CancellationToken ct = default);
    void Remove(AiKnowledgeChunk chunk);
    void RemoveRange(IEnumerable<AiKnowledgeChunk> chunks);
}
