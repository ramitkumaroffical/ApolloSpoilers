using ApolloSpoilers.Domain.Interfaces.Ai;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

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
        var baseUrl = config["Ai:Llm:BaseUrl"] ?? "http://localhost:11434/v1";
        var apiKey = config["Ai:Llm:ApiKey"] ?? "ollama-local";
        _model = config["Ai:Llm:Model"] ?? "llama3";

        // Honor the configured chat timeout (default 120s). The HttpClient default
        // of 100s is too short for CPU-bound local LLMs (e.g. llama3 on Ollama).
        var timeoutSeconds = int.TryParse(config["Ai:Chat:TimeoutSeconds"], out var t) && t > 0 ? t : 120;
        var http = new HttpClient { BaseAddress = new Uri(baseUrl), Timeout = TimeSpan.FromSeconds(timeoutSeconds) };

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
}
