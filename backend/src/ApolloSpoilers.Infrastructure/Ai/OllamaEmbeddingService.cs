using ApolloSpoilers.Domain.Interfaces.Ai;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

public class OllamaEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _http;
    private readonly ILogger<OllamaEmbeddingService> _logger;

    public int VectorSize => 768;

    public OllamaEmbeddingService(IConfiguration config, ILogger<OllamaEmbeddingService> logger)
    {
        var baseUrl = config["Ai:Embedding:BaseUrl"] ?? config["Ai__Embedding__BaseUrl"] ?? "http://localhost:11434";
        var endpointPath = config["Ai:Embedding:EndpointPath"] ?? "/api/embeddings";

        _logger = logger;
        _http = new HttpClient
        {
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };

        _logger.LogInformation("OllamaEmbeddingService initialized with baseUrl: {BaseUrl}, endpointPath: {EndpointPath}",
            baseUrl, endpointPath);
    }

    public async Task<ReadOnlyMemory<float>> EmbedAsync(string text, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/embeddings", new
            {
                model = "nomic-embed-text",
                prompt = text
            }, ct);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Ollama embedding failed with status {Status}: {Error}",
                    response.StatusCode, errorContent);

                // Return empty vector if Ollama is unavailable
                _logger.LogWarning("Returning empty embedding vector due to Ollama service unavailability");
                return Array.Empty<float>();
            }

            var result = await response.Content.ReadFromJsonAsync<OllamaResponse>(ct);

            if (result?.embedding == null || result.embedding.Length == 0)
            {
                _logger.LogWarning("Ollama returned empty embedding vector");
                return Array.Empty<float>();
            }

            return result.embedding;
        }
        catch (HttpRequestException ex) when (ex.StatusCode is System.Net.HttpStatusCode.BadGateway ||
                                                ex.StatusCode is System.Net.HttpStatusCode.ServiceUnavailable ||
                                                ex.StatusCode is System.Net.HttpStatusCode.GatewayTimeout)
        {
            _logger.LogError(ex, "Ollama service unavailable (Bad Gateway/Service Unavailable/Gateway Timeout). The remote Ollama service may be down or overloaded.");

            // Return empty vector as graceful fallback
            _logger.LogWarning("Returning empty embedding vector due to Ollama service unavailability");
            return Array.Empty<float>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ollama embedding service request failed");
            return Array.Empty<float>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in Ollama embedding service");
            return Array.Empty<float>();
        }
    }

    public async Task<IReadOnlyList<ReadOnlyMemory<float>>> EmbedBatchAsync(
        IEnumerable<string> texts,
        CancellationToken ct = default)
    {
        var list = new List<ReadOnlyMemory<float>>();

        foreach (var t in texts)
        {
            list.Add(await EmbedAsync(t, ct));
        }

        return list;
    }

    private class OllamaResponse
    {
        public float[] embedding { get; set; } = [];
    }
}