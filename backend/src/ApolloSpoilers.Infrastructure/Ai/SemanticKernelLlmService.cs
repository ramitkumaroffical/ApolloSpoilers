using ApolloSpoilers.Domain.Interfaces.Ai;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Net.Http.Headers;

namespace ApolloSpoilers.Infrastructure.Ai;

/// <summary>
/// OpenAI-compatible chat completion service backed by Semantic Kernel.
/// Default config points at local Ollama (llama3); swap env vars for Groq/OpenAI.
/// </summary>
public class SemanticKernelLlmService : ILlmService
{
    private readonly IChatCompletionService _chat;
    private readonly string _model;

    public SemanticKernelLlmService(IConfiguration config)
    {
        // FIX: Added Environment Variables compatibility (Double Underscores fallback)
        var baseUrl = config["Ai__Llm__BaseUrl"] ?? config["Ai:Llm:BaseUrl"] ?? "https://api.groq.com/openai/v1";
        var apiKey = config["Ai__Llm__ApiKey"] ?? config["Ai:Llm:ApiKey"] ?? "ollama-local";
        _model = config["Ai__Llm__Model"] ?? config["Ai:Llm:Model"] ?? "llama-3.3-70b-versatile";

        // FIX: Config timeout fallbacks for production env
        var timeoutSecondsStr = config["Ai__Chat__TimeoutSeconds"] ?? config["Ai:Chat:TimeoutSeconds"];
        var timeoutSeconds = int.TryParse(timeoutSecondsStr, out var t) && t > 0 ? t : 120;

        // FIX: Passing apiKey to attach the Authorization Bearer Header for Groq Cloud
        var http = CreateHttpClient(baseUrl, apiKey, timeoutSeconds);

        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion(modelId: _model, apiKey: apiKey, httpClient: http);
        var kernel = builder.Build();
        _chat = kernel.GetRequiredService<IChatCompletionService>();
    }

    public async Task<string> CompleteAsync(IEnumerable<LlmMessage> messages, CancellationToken ct = default)
    {
        var history = new ChatHistory();
        foreach (var m in messages)
        {
            switch (m.Role)
            {
                case LlmRole.System:
                    history.AddSystemMessage(m.Content);
                    break;
                case LlmRole.Assistant:
                    history.AddAssistantMessage(m.Content);
                    break;
                default:
                    history.AddUserMessage(m.Content);
                    break;
            }
        }

        var result = await _chat.GetChatMessageContentAsync(history, cancellationToken: ct);
        return result.Content ?? string.Empty;
    }

    // FIX: Created an authenticating HttpClient that attaches the Bearer token for Groq API
    private static HttpClient CreateHttpClient(string baseUrl, string apiKey, int timeoutSeconds)
    {
        var http = new HttpClient
        {
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromSeconds(timeoutSeconds)
        };

        if (!string.IsNullOrEmpty(apiKey) && apiKey != "ollama-local")
        {
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        }

        return http;
    }
}