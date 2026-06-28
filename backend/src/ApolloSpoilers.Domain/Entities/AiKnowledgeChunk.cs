using ApolloSpoilers.Domain.Common;
using ApolloSpoilers.Domain.Enums;

namespace ApolloSpoilers.Domain.Entities;

/// <summary>
/// Pointer from a piece of knowledge to its vector in Qdrant. The actual
/// vector lives in Qdrant; this row is the SQL-side record of what we indexed.
/// </summary>
public class AiKnowledgeChunk : BaseEntity
{
    public Guid? ProductId { get; set; }
    public Product? Product { get; set; }

    public Guid? CategoryId { get; set; }

    public KnowledgeSourceType SourceType { get; set; }

    public string ChunkText { get; set; } = string.Empty;

    /// <summary>The Qdrant point id this chunk maps to.</summary>
    public Guid QdrantPointId { get; set; }

    /// <summary>Optional metadata payload (JSON) stored alongside the vector.</summary>
    public string? Metadata { get; set; }
}
