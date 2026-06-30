using ApolloSpoilers.Domain.Interfaces.Ai;
using System.Net.Http.Json;

public class OllamaEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _http;

    public int VectorSize => 768;

    public OllamaEmbeddingService()
    {
        _http = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:11434")
        };
    }

    public async Task<ReadOnlyMemory<float>> EmbedAsync(string text, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("/api/embeddings", new
        {
            model = "nomic-embed-text",
            prompt = text
        }, ct);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaResponse>(ct);

        return result?.embedding ?? [];
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