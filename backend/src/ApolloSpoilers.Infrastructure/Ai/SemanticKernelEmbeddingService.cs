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
            config["Ai__Embedding__BaseUrl"]
            ?? "https://api-atlas.nomic.ai/v1";

        var apiKey =
            config["Ai__Embedding__ApiKey"];

        _http = new HttpClient();

        _http.BaseAddress = new Uri(baseUrl);

        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                apiKey);
    }


    public int VectorSize => 768;


    public async Task<ReadOnlyMemory<float>> EmbedAsync(
        string text,
        CancellationToken ct = default)
    {
        try
        {
            var body = new
            {
                model = "nomic-embed-text-v1.5",
                input = new[]
                {
                    text
                }
            };


            var response =
                await _http.PostAsJsonAsync(
                    "/embeddings",
                    body,
                    ct);


            response.EnsureSuccessStatusCode();


            var result =
                await response.Content.ReadFromJsonAsync<NomicResponse>(ct);


            return result!
                .data[0]
                .embedding;

        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Embedding generation failed");

            throw;
        }
    }


    public async Task<IReadOnlyList<ReadOnlyMemory<float>>> EmbedBatchAsync(
        IEnumerable<string> texts,
        CancellationToken ct = default)
    {
        var result = new List<ReadOnlyMemory<float>>();

        foreach (var text in texts)
        {
            result.Add(
                await EmbedAsync(text, ct));
        }

        return result;
    }


    private class NomicResponse
    {
        public List<DataItem> data { get; set; } = new();
    }


    private class DataItem
    {
        public float[] embedding { get; set; } = Array.Empty<float>();
    }
}