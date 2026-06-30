using ApolloSpoilers.Domain.Interfaces.Ai;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ApolloSpoilers.Infrastructure.Ai;

public class SemanticKernelEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _http;
    private readonly ILogger<SemanticKernelEmbeddingService> _logger;

    public SemanticKernelEmbeddingService(
        IConfiguration config,
        ILogger<SemanticKernelEmbeddingService> logger)
    {
        _logger = logger;

        var baseUrl =
            config["Ai:Embedding:BaseUrl"]
            ?? "https://api-atlas.nomic.ai/v1";

        var apiKey =
            config["Ai:Embedding:ApiKey"];

        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri))
            throw new InvalidOperationException(
                $"Invalid Ai:Embedding:BaseUrl value: '{baseUrl}'. It must be an absolute URI like 'https://api-atlas.nomic.ai/v1'.");

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException(
                "Ai:Embedding:ApiKey is missing or empty.");

        _http = new HttpClient
        {
            BaseAddress = new Uri(baseUri.ToString().TrimEnd('/') + "/")
        };

        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);
    }

    public int VectorSize => 768;

    public async Task<ReadOnlyMemory<float>> EmbedAsync(
        string text,
        CancellationToken ct = default)
    {
        var results = await EmbedBatchAsync(new[] { text }, ct);
        return results[0];
    }

    public async Task<IReadOnlyList<ReadOnlyMemory<float>>> EmbedBatchAsync(
        IEnumerable<string> texts,
        CancellationToken ct = default)
    {
        try
        {
            var textArray = texts?.Where(t => !string.IsNullOrWhiteSpace(t)).ToArray()
                ?? Array.Empty<string>();

            if (textArray.Length == 0)
                return Array.Empty<ReadOnlyMemory<float>>();

            var body = new
            {
                model = "nomic-embed-text-v1.5",
                texts = textArray,
                task_type = "search_document"
            };

            using var response = await _http.PostAsJsonAsync("embedding/text", body, ct);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<NomicResponse>(ct);

            if (result?.embeddings == null)
                throw new InvalidOperationException("Embedding API returned no embeddings.");

            return result.embeddings
                .Select(e => new ReadOnlyMemory<float>(e))
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Embedding generation failed");
            throw;
        }
    }

    private class NomicResponse
    {
        public List<float[]> embeddings { get; set; } = new();
    }
}