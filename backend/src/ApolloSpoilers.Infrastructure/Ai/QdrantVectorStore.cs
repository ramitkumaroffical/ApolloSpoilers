using ApolloSpoilers.Domain.Interfaces.Ai;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace ApolloSpoilers.Infrastructure.Ai;

/// <summary>
/// Qdrant-backed implementation of <see cref="IVectorStore"/>. Manages a single
/// collection (default "apollo_products") and exposes upsert/search/delete.
/// </summary>
public class QdrantVectorStore : IVectorStore
{
    private readonly QdrantClient _client;
    private readonly string _collection;
    private readonly ILogger<QdrantVectorStore> _logger;

    public QdrantVectorStore(IConfiguration config, ILogger<QdrantVectorStore> logger)
    {
        var endpoint = config["Ai:Qdrant:Endpoint"]
                             ?? "http://localhost:6333";

        var uri = new Uri(endpoint);

        int grpcPort = 6334;

        if (int.TryParse(config["Ai:Qdrant:Port"], out var configuredGrpcPort)
            && configuredGrpcPort > 0)
        {
            grpcPort = configuredGrpcPort;
        }

        var apiKey = config["Ai:Qdrant:ApiKey"];

        _client = new QdrantClient(
            host: uri.Host,
            port: grpcPort,
            https: uri.Scheme == "https",
            apiKey: apiKey
        );

        _collection = config["Ai:Qdrant:Collection"] ?? "apollo_products";
        _logger = logger;

    }

    public async Task EnsureCollectionAsync(int vectorSize, CancellationToken ct = default)
    {
        var exists = await _client.CollectionExistsAsync(_collection, cancellationToken: ct);
        if (exists) return;

        await _client.CreateCollectionAsync(
            collectionName: _collection,
            vectorsConfig: new VectorParams { Size = (ulong)vectorSize, Distance = Distance.Cosine },
            cancellationToken: ct);

        _logger.LogInformation("Created Qdrant collection '{Collection}' (dim={Dim})", _collection, vectorSize);
    }

    public async Task UpsertAsync(Guid pointId, ReadOnlyMemory<float> vector, IReadOnlyDictionary<string, object?> payload, CancellationToken ct = default)
    {
        var pointStruct = new PointStruct
        {
            Id = pointId,
            Vectors = vector.ToArray(),
            Payload = { }
        };

        foreach (var kvp in payload)
        {
            if (kvp.Value is not null)
                pointStruct.Payload.Add(kvp.Key, ToValue(kvp.Value));
        }

        await _client.UpsertAsync(_collection, new[] { pointStruct }, cancellationToken: ct);
    }

    public async Task UpsertBatchAsync(
        IEnumerable<(Guid PointId, ReadOnlyMemory<float> Vector, IReadOnlyDictionary<string, object?> Payload)> points,
        CancellationToken ct = default)
    {
        var batch = new List<PointStruct>();
        foreach (var p in points)
        {
            var ps = new PointStruct
            {
                Id = p.PointId,
                Vectors = p.Vector.ToArray(),
                Payload = { }
            };
            foreach (var kvp in p.Payload)
            {
                if (kvp.Value is not null)
                    ps.Payload.Add(kvp.Key, ToValue(kvp.Value));
            }
            batch.Add(ps);
        }

        if (batch.Count == 0) return;
        await _client.UpsertAsync(_collection, batch, cancellationToken: ct);
    }

    public async Task DeleteAsync(Guid pointId, CancellationToken ct = default)
        => await _client.DeleteAsync(_collection, pointId, cancellationToken: ct);

    public async Task DeleteBatchAsync(IEnumerable<Guid> pointIds, CancellationToken ct = default)
    {
        var ids = pointIds.ToList();
        if (ids.Count == 0) return;
        await _client.DeleteAsync(_collection, ids, cancellationToken: ct);
    }

    public async Task<IReadOnlyList<VectorSearchHit>> SearchAsync(ReadOnlyMemory<float> queryVector, int topK, CancellationToken ct = default)
    {
        var results = await _client.SearchAsync(
            collectionName: _collection,
            vector: queryVector.ToArray(),
            limit: (ulong)topK,
            payloadSelector: true,
            cancellationToken: ct);

        var hits = new List<VectorSearchHit>();
        foreach (var r in results)
        {
            var payloadDict = new Dictionary<string, object?>();
            foreach (var kv in r.Payload)
                payloadDict[kv.Key] = ExtractObject(kv.Value);

            hits.Add(new VectorSearchHit(
                PointId: Guid.TryParse(r.Id.Uuid, out var pid) ? pid : Guid.Empty,
                Score: r.Score,
                Text: ExtractString(r.Payload, "text") ?? string.Empty,
                Payload: payloadDict));
        }
        return hits;
    }

    // --- Value conversion helpers ---
    // Value has implicit operators from string, long, bool, double only.

    private static Value ToValue(object o) => o switch
    {
        string s => s,
        int i => (long)i,
        long l => l,
        double d => d,
        float f => (double)f,
        bool b => b,
        Guid g => g.ToString(),
        decimal dec => (double)dec,
        _ => o.ToString() ?? string.Empty
    };

    private static string? ExtractString(IDictionary<string, Value> payload, string key)
        => payload.TryGetValue(key, out var v) && v.HasStringValue ? v.StringValue : null;

    private static object? ExtractObject(Value v) => v.KindCase switch
    {
        Value.KindOneofCase.StringValue => v.StringValue,
        Value.KindOneofCase.IntegerValue => v.IntegerValue,
        Value.KindOneofCase.DoubleValue => v.DoubleValue,
        Value.KindOneofCase.BoolValue => v.BoolValue,
        _ => v.ToString()
    };
}
