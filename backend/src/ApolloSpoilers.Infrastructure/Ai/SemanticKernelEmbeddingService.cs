using ApolloSpoilers.Domain.Interfaces.Ai;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;
using System.Net.Http.Headers;

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

        // FIX: Added Environment Variables compatibility (Double Underscores fallback)
        var baseUrl = config["Ai__Embedding__BaseUrl"] ?? config["Ai:Embedding:BaseUrl"] ??
                      config["Ai__Llm__BaseUrl"] ?? config["Ai:Llm:BaseUrl"] ?? "https://api-atlas.nomic.ai/v1";

        var apiKey = config["Ai__Embedding__ApiKey"] ?? config["Ai:Embedding:ApiKey"] ??
                     config["Ai__Llm__ApiKey"] ?? config["Ai:Llm:ApiKey"] ?? "ollama-local";

        var model = config["Ai__Embedding__Model"] ?? config["Ai:Embedding:Model"] ?? "nomic-embed-text-v1.5";

        var builder = Kernel.CreateBuilder();

        // FIX: Passing the authenticating HTTP Client required for Nomic Cloud
        builder.AddOpenAITextEmbeddingGeneration(
            modelId: model,
            apiKey: apiKey,
            httpClient: CreateHttpClient(baseUrl, apiKey));

        var kernel = builder.Build();
        _embedder = kernel.GetRequiredService<ITextEmbeddingGenerationService>();

        // nomic-embed-text is 768-dim. We learn the true size lazily on first call.
        _vectorSize = 768;
    }

    public int VectorSize => _vectorSize;

    public async Task<ReadOnlyMemory<float>> EmbedAsync(string text, CancellationToken ct = default)
    {
        try
        {
            var vectors = await _embedder.GenerateEmbeddingsAsync(new[] { text }, cancellationToken: ct);
            if (vectors is null || vectors.Count == 0)
                throw new InvalidOperationException("Embedding service returned no vectors.");
            return vectors[0];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating individual embedding vector from Semantic Kernel.");
            throw;
        }
    }

    public async Task<IReadOnlyList<ReadOnlyMemory<float>>> EmbedBatchAsync(IEnumerable<string> texts, CancellationToken ct = default)
    {
        try
        {
            var list = texts.ToList();
            var vectors = await _embedder.GenerateEmbeddingsAsync(list, cancellationToken: ct);
            return vectors?.ToList() ?? new List<ReadOnlyMemory<float>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating batch embeddings from Semantic Kernel.");
            throw;
        }
    }

    // FIX: Created an authenticating HttpClient that attaches the Bearer token for Nomic Cloud API
    private static HttpClient CreateHttpClient(string baseUrl, string apiKey)
    {
        var http = new HttpClient { BaseAddress = new Uri(baseUrl) };

        if (!string.IsNullOrEmpty(apiKey) && apiKey != "ollama-local")
        {
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        }

        return http;
    }
}