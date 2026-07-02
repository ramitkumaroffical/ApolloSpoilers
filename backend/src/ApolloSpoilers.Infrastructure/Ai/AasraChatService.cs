using System.Text.Json;
using ApolloSpoilers.Application.Common;
using ApolloSpoilers.Application.DTOs;
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
/// Aasra — Apollo Spoilers' AI assistant. Orchestrates the full RAG pipeline:
/// embed user query → search Qdrant → assemble prompt with retrieved context →
/// generate answer via Semantic Kernel LLM → persist the conversation turn.
/// </summary>
public class AasraChatService : IAasraChatService
{
    private const string SystemPersona = """
        You are Aasra, the AI shopping assistant for Apollo Spoilers, an online store
        that sells car spoilers, GT wings, front splitters, lip spoilers, and automotive
        exterior styling accessories.

        Your job:
        - Recommend suitable products based on the customer's car and preferences.
        - Verify car compatibility using the retrieved product data.
        - Help with product search, comparisons, and order-related questions.
        - Be concise, friendly, and helpful. Use bullet points where useful.

        Rules:
        - Base answers ONLY on the provided retrieved context. If the context is empty
          or insufficient, say you don't have enough information rather than inventing facts.
        - When recommending a product, mention its name, price, and which cars it fits.
        - Never quote a price you did not see in the context.
        - If a customer asks about their order and you lack access to their order data,
          tell them to check the Orders page.
        """;

    private readonly IUnitOfWork _uow;
    private readonly IEmbeddingService _embedder;
    private readonly IVectorStore _vectorStore;
    private readonly ILlmService _llm;
    private readonly IConfiguration _config;
    private readonly ILogger<AasraChatService> _logger;

    public AasraChatService(
        IUnitOfWork uow,
        IEmbeddingService embedder,
        IVectorStore vectorStore,
        ILlmService llm,
        IConfiguration config,
        ILogger<AasraChatService> logger)
    {
        _uow = uow;
        _embedder = embedder;
        _vectorStore = vectorStore;
        _llm = llm;
        _config = config;
        _logger = logger;
    }

    public async Task<Result<ChatResponseDto>> SendMessageAsync(Guid? userId, SendMessageDto dto, CancellationToken ct = default)
    {
        ChatSession? session = null;
        if (userId.HasValue)
        {
            if (dto.SessionId.HasValue)
            {
                session = (await _uow.Repository<ChatSession>().ListAsync(new ChatSessionByIdSpecification(dto.SessionId.Value), ct)).FirstOrDefault()
                          ?? throw new KeyNotFoundException("Chat session not found.");
            }
            else
            {
                session = new ChatSession { UserId = userId.Value, Title = Truncate(dto.Message, 50) };
                await _uow.Repository<ChatSession>().AddAsync(session, ct);
                await _uow.SaveChangesAsync(ct);
            }

            // 1. Persist the user turn
            await _uow.Repository<ChatMessage>().AddAsync(new ChatMessage
            {
                SessionId = session.Id,
                Role = ChatRole.User,
                Content = dto.Message
            }, ct);
            await _uow.SaveChangesAsync(ct);
        }

        string answer;
        IReadOnlyList<ChatSourceDto> sources = Array.Empty<ChatSourceDto>();

        try
        {
            // 2. RAG retrieval
            await _vectorStore.EnsureCollectionAsync(_embedder.VectorSize, ct);
            var queryVector = await _embedder.EmbedAsync(dto.Message, ct);

            // If embedding failed or returned empty vector, provide a fallback response
            if (queryVector.Length == 0)
            {
                _logger.LogWarning("Embedding failed, providing fallback response without RAG");

                var fallbackMessage = $"I apologize, but I'm having trouble accessing my knowledge base right now. " +
                                      "Please try again in a moment, or browse our catalog manually. " +
                                      "If this issue persists, please contact support.";

                return Result.Success(new ChatResponseDto
                {
                    SessionId = session?.Id ?? Guid.Empty,
                    Answer = fallbackMessage,
                    Sources = Array.Empty<ChatSourceDto>()
                });
            }

            var topK = int.Parse(_config["Ai:Chat:TopK"] ?? "5");
            var hits = await _vectorStore.SearchAsync(queryVector, topK, ct);

            sources = hits.Select(h => new ChatSourceDto
            {
                Type = h.Payload.TryGetValue("sourceType", out var t) ? t?.ToString() ?? "product" : "product",
                ProductId = h.Payload.TryGetValue("productId", out var pid) ? Guid.TryParse(pid?.ToString(), out var g) ? g : null : null,
                ProductSlug = h.Payload.TryGetValue("productSlug", out var ps) ? ps?.ToString() : null,
                ProductName = h.Payload.TryGetValue("productName", out var pn) ? pn?.ToString() : null,
                Score = h.Score
            }).ToList();

            // 3. Build the augmented prompt
            var contextBlock = BuildContextBlock(hits);
            var history = session is null
                ? Array.Empty<LlmMessage>()
                : await BuildHistoryAsync(session.Id, ct);

            var messages = new List<LlmMessage>
            {
                new(LlmRole.System, SystemPersona),
                new(LlmRole.System, $"Retrieved product context:\n{contextBlock}")
            };
            messages.AddRange(history);
            messages.Add(new LlmMessage(LlmRole.User, dto.Message));

            // 4. Generate
            answer = await _llm.CompleteAsync(messages, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Aasra pipeline failure for session {SessionId}", session?.Id);
            answer = "I'm sorry, I couldn't reach my knowledge base right now. Please try again in a moment.";
        }

        // 5. Persist the assistant turn (authenticated users only)
        if (session is not null)
        {
            await _uow.Repository<ChatMessage>().AddAsync(new ChatMessage
            {
                SessionId = session.Id,
                Role = ChatRole.Assistant,
                Content = answer,
                Sources = JsonSerializer.Serialize(sources)
            }, ct);
            await _uow.SaveChangesAsync(ct);
        }

        return Result.Success(new ChatResponseDto
        {
            SessionId = session?.Id ?? Guid.Empty,
            Answer = answer,
            Sources = sources
        });
    }

    public async Task<IReadOnlyList<ChatMessageDto>> GetHistoryAsync(Guid sessionId, CancellationToken ct = default)
    {
        var session = (await _uow.Repository<ChatSession>().ListAsync(new ChatSessionByIdSpecification(sessionId), ct)).FirstOrDefault();
        if (session is null) return Array.Empty<ChatMessageDto>();

        return session.Messages
            .OrderBy(m => m.CreatedAt)
            .Select(m => new ChatMessageDto
            {
                Id = m.Id,
                Role = m.Role.ToString().ToLowerInvariant(),
                Content = m.Content,
                CreatedAt = m.CreatedAt
            })
            .ToList();
    }

    private async Task<IReadOnlyList<LlmMessage>> BuildHistoryAsync(Guid sessionId, CancellationToken ct)
    {
        var session = (await _uow.Repository<ChatSession>().ListAsync(new ChatSessionByIdSpecification(sessionId), ct)).FirstOrDefault();
        if (session is null) return Array.Empty<LlmMessage>();

        var historyCount = int.Parse(_config["Ai:Chat:HistoryMessages"] ?? "10");
        return session.Messages
            .OrderByDescending(m => m.CreatedAt)
            .Take(historyCount)
            .Reverse()
            .Select(m => new LlmMessage(
                m.Role == ChatRole.User ? LlmRole.User : LlmRole.Assistant,
                m.Content))
            .ToList();
    }

    private static string BuildContextBlock(IReadOnlyList<VectorSearchHit> hits)
    {
        if (hits.Count == 0) return "(No relevant product information was found.)";

        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < hits.Count; i++)
        {
            var h = hits[i];
            sb.AppendLine($"--- Product {i + 1} (relevance {h.Score:P0}) ---");
            sb.AppendLine(h.Text);
            sb.AppendLine();
        }
        return sb.ToString();
    }

    private static string Truncate(string s, int max) =>
        s.Length <= max ? s : s.Substring(0, max);
}
