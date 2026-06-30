using ApolloSpoilers.Domain.Interfaces.Ai;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;

namespace ApolloSpoilers.Infrastructure.Ai;

public class SemanticKernelEmbeddingService : IEmbeddingService
{
    private readonly ITextEmbeddingGenerationService _embedder;
    private readonly ILogger<SemanticKernelEmbeddingService> _logger;

    public SemanticKernelEmbeddingService(
        IConfiguration config,
        ILogger<SemanticKernelEmbeddingService> logger)
    {
        _logger = logger;

        var baseUrl =
            config["Ai:Embedding:BaseUrl"]
            ?? "https://api-inference.huggingface.co/v1";

        var apiKey =
            config["Ai:Embedding:ApiKey"]
            ?? throw new InvalidOperationException(
                "Embedding API key missing.");

        var model =
            config["Ai:Embedding:Model"]
            ?? "sentence-transformers/all-MiniLM-L6-v2";


        var builder = Kernel.CreateBuilder();

        builder.AddOpenAITextEmbeddingGeneration(
            modelId: model,
            apiKey: apiKey,
            httpClient: CreateHttpClient(baseUrl)
        );

        var kernel = builder.Build();

        _embedder =
            kernel.GetRequiredService<ITextEmbeddingGenerationService>();
    }


    public int VectorSize => 384;


    public async Task<ReadOnlyMemory<float>> EmbedAsync(
        string text,
        CancellationToken ct = default)
    {
        var vectors =
            await _embedder.GenerateEmbeddingsAsync(
                new[] { text },
                cancellationToken: ct);


        if (vectors == null || vectors.Count == 0)
            throw new InvalidOperationException(
                "Embedding service returned no vectors.");


        return vectors[0];
    }


    public async Task<IReadOnlyList<ReadOnlyMemory<float>>> EmbedBatchAsync(
        IEnumerable<string> texts,
        CancellationToken ct = default)
    {
        var list = texts.ToList();

        var vectors =
            await _embedder.GenerateEmbeddingsAsync(
                list,
                cancellationToken: ct);


        return vectors?.ToList()
            ?? new List<ReadOnlyMemory<float>>();
    }


    private static HttpClient CreateHttpClient(string baseUrl)
    {
        var http = new HttpClient
        {
            BaseAddress = new Uri(baseUrl)
        };

        return http;
    }
}