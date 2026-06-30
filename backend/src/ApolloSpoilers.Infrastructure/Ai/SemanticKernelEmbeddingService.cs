using ApolloSpoilers.Domain.Interfaces.Ai;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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

        _model = config["Ai:Embedding:Model"]
                 ?? "nomic-ai/nomic-embed-text-v1.5";


        _http = new HttpClient();

        _http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                config["Ai:Embedding:ApiKey"]);
    }


    public async Task<ReadOnlyMemory<float>> EmbedAsync(
        string text,
        CancellationToken ct = default)
    {

        var body = new
        {
            inputs = text
        };


        var response = await _http.PostAsJsonAsync(
            $"https://api-inference.huggingface.co/models/{_model}",
            body,
            ct);


        response.EnsureSuccessStatusCode();


        var result =
            await response.Content.ReadFromJsonAsync<float[][]>(ct);


        if (result == null || result.Length == 0)
            throw new Exception("No embedding returned");


        return result[0];
    }



    public async Task<IReadOnlyList<ReadOnlyMemory<float>>> EmbedBatchAsync(
        IEnumerable<string> texts,
        CancellationToken ct = default)
    {

        var list = texts.ToList();

        var response = await _http.PostAsJsonAsync(
            $"https://api-inference.huggingface.co/models/{_model}",
            new { inputs = list },
            ct);


        response.EnsureSuccessStatusCode();


        var result =
            await response.Content.ReadFromJsonAsync<float[][]>(ct);


        return result?
            .Select(x => new ReadOnlyMemory<float>(x))
            .ToList()
            ?? [];
    }
}