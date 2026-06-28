namespace ApolloSpoilers.Domain.Interfaces.Ai;

/// <summary>Role of a message in an LLM chat completion request.</summary>
public enum LlmRole { System, User, Assistant }

public record LlmMessage(LlmRole Role, string Content);

/// <summary>OpenAI-compatible chat completion abstraction (works for Ollama, Groq, OpenAI, etc.).</summary>
public interface ILlmService
{
    /// <summary>Generate a single completion for the given message list.</summary>
    Task<string> CompleteAsync(IEnumerable<LlmMessage> messages, CancellationToken ct = default);
}
