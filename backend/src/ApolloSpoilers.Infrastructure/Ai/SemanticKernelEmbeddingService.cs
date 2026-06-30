using ApolloSpoilers.Domain.Interfaces.Ai;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Net.Http.Json;

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

        // HuggingFace embedding model
        _model = config["Ai:Embedding:Model"]
                 ?? "sentence-transformers/all-MiniLM-L6-v2";

        var apiKey = config["Ai:Embedding:ApiKey"];

        _http = new HttpClient
        {
            BaseAddress = new Uri("https://api-inference.huggingface.co")
        };

        if (!string.IsNullOrEmpty(apiKey))
        {
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);
        }
    }

    public async Task<ReadOnlyMemory<float>> EmbedAsync(
        string text,
        CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync(
            $"/pipeline/feature-extraction/{_model}",
            new { inputs = text },
            ct);

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("HF embedding failed: {Error}", err);
            throw new Exception($"Embedding failed: {err}");
        }

        var result =
            await response.Content.ReadFromJsonAsync<float[][]>(ct);

        if (result == null || result.Length == 0)
            throw new Exception("No embedding returned from HuggingFace");

        return result[0];
    }

    public async Task<IReadOnlyList<ReadOnlyMemory<float>>> EmbedBatchAsync(
        IEnumerable<string> texts,
        CancellationToken ct = default)
    {
        var list = texts.ToList();

        var response = await _http.PostAsJsonAsync(
            $"/pipeline/feature-extraction/{_model}",
            new { inputs = list },
            ct);

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("HF batch embedding failed: {Error}", err);
            throw new Exception($"Batch embedding failed: {err}");
        }

        var result =
            await response.Content.ReadFromJsonAsync<float[][]>(ct);

        return result?
            .Select(x => new ReadOnlyMemory<float>(x))
            .ToList()
            ?? new List<ReadOnlyMemory<float>>();
    }
}