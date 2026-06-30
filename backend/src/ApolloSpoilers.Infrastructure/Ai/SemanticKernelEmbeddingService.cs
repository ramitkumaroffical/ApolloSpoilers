using System.Net.Http.Headers;
using System.Net.Http.Json;
using ApolloSpoilers.Domain.Interfaces.Ai;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public class SemanticKernelEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _http;
    private readonly ILogger<SemanticKernelEmbeddingService> _logger;

    private readonly string _model;

    public int VectorSize => 768;

    public SemanticKernelEmbeddingService(
        IConfiguration config,
        ILogger<SemanticKernelEmbeddingService> logger)
    {
        _logger = logger;

        _model = config["Ai:Embedding:Model"]
                 ?? "nomic-ai/nomic-embed-text-v1.5";

        _http = new HttpClient
        {
            BaseAddress = new Uri("https://router.huggingface.co")
        };

        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                config["Ai:Embedding:ApiKey"]);
    }

    public async Task<ReadOnlyMemory<float>> EmbedAsync(string text, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.PostAsJsonAsync(
                "/v1/embeddings",
                new
                {
                    model = _model,
                    input = text
                },
                ct);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<EmbeddingResponse>(ct);

            if (result?.data == null || result.data.Length == 0)
                throw new Exception("No embedding returned");

            return result.data[0].embedding;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Embedding failed");
            throw;
        }
    }

    public async Task<IReadOnlyList<ReadOnlyMemory<float>>> EmbedBatchAsync(
        IEnumerable<string> texts,
        CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync(
            "/v1/embeddings",
            new
            {
                model = _model,
                input = texts.ToList()
            },
            ct);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<EmbeddingResponse>(ct);

        return result?.data?
            .Select(x => new ReadOnlyMemory<float>(x.embedding))
            .ToList()
            ?? [];
    }

    private class EmbeddingResponse
    {
        public EmbeddingItem[] data { get; set; } = [];
    }

    private class EmbeddingItem
    {
        public float[] embedding { get; set; } = [];
    }
}