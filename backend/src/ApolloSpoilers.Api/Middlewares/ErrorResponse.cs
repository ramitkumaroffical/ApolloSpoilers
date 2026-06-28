namespace ApolloSpoilers.Api.Middlewares;

/// <summary>
/// Consistent error response shape returned for all non-2xx responses.
/// </summary>
public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
    public string? TraceId { get; set; }
    public IReadOnlyDictionary<string, string[]>? Errors { get; set; }
}
