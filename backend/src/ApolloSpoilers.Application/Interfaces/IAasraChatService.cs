using ApolloSpoilers.Application.Common;
using ApolloSpoilers.Application.DTOs;

namespace ApolloSpoilers.Application.Interfaces;

/// <summary>
/// High-level Aasra chat orchestration. Application layer defines this; the
/// Infrastructure layer implements it using Semantic Kernel + Qdrant.
/// </summary>
public interface IAasraChatService
{
    Task<Result<ChatResponseDto>> SendMessageAsync(Guid? userId, SendMessageDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<ChatMessageDto>> GetHistoryAsync(Guid sessionId, CancellationToken ct = default);
}

/// <summary>
/// Keeps the Qdrant knowledge base in sync with the catalog. Called by admin
/// product operations and by the seed routine.
/// </summary>
public interface IProductIndexer
{
    Task IndexProductAsync(Guid productId, CancellationToken ct = default);
    Task RemoveFromIndexAsync(Guid productId, CancellationToken ct = default);
    Task ReindexAllAsync(CancellationToken ct = default);
}
