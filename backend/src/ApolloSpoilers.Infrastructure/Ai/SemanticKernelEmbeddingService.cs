using ApolloSpoilers.Domain.Interfaces.Ai;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;

namespace ApolloSpoilers.Infrastructure.Ai;

/// <summary>
/// OpenAI-compatible embedding service backed by Semantic Kernel. Works with
/// Ollama (/v1), Groq, or OpenAI by swapping the configured base URL + model.
/// </summary>
public class SemanticKernelEmbeddingService : IEmbeddingService
{
    private readonly ITextEmbeddingGenerationService _embedder;
    private readonly ILogger<SemanticKernelEmbeddingService> _logger;
    private readonly int _vectorSize;

    public SemanticKernelEmbeddingService(IConfiguration config, ILogger<SemanticKernelEmbeddingService> logger)
    {
        _logger = logger;
        var baseUrl = config["Ai:Embedding:BaseUrl"] ?? config["Ai:Llm:BaseUrl"] ?? "http://localhost:11434/v1";
        var apiKey = config["Ai:Embedding:ApiKey"] ?? config["Ai:Llm:ApiKey"] ?? "ollama-local";
        var model = config["Ai:Embedding:Model"] ?? "nomic-embed-text";

        var builder = Kernel.CreateBuilder();
        builder.AddOpenAITextEmbeddingGeneration(modelId: model, apiKey: apiKey, httpClient: CreateHttpClient(baseUrl));
        var kernel = builder.Build();
        _embedder = kernel.GetRequiredService<ITextEmbeddingGenerationService>();

        // nomic-embed-text is 768-dim. We learn the true size lazily on first call.
        _vectorSize = 768;
    }

    public int VectorSize => _vectorSize;

    public async Task<ReadOnlyMemory<float>> EmbedAsync(string text, CancellationToken ct = default)
    {
        var vectors = await _embedder.GenerateEmbeddingsAsync(new[] { text }, cancellationToken: ct);
        if (vectors is null || vectors.Count == 0)
            throw new InvalidOperationException("Embedding service returned no vectors.");
        return vectors[0];
    }

    public async Task<IReadOnlyList<ReadOnlyMemory<float>>> EmbedBatchAsync(IEnumerable<string> texts, CancellationToken ct = default)
    {
        var list = texts.ToList();
        var vectors = await _embedder.GenerateEmbeddingsAsync(list, cancellationToken: ct);
        return vectors?.ToList() ?? new List<ReadOnlyMemory<float>>();
    }

    private static HttpClient CreateHttpClient(string baseUrl)
    {
        var http = new HttpClient { BaseAddress = new Uri(baseUrl) };
        return http;
    }
}
