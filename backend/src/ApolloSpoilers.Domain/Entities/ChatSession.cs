using ApolloSpoilers.Domain.Common;
using ApolloSpoilers.Domain.Enums;

namespace ApolloSpoilers.Domain.Entities;

/// <summary>A conversation thread between a user and Aasra.</summary>
public class ChatSession : BaseEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public string Title { get; set; } = "New conversation";

    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}

public class ChatMessage : BaseEntity
{
    public Guid SessionId { get; set; }
    public ChatSession? Session { get; set; }

    public ChatRole Role { get; set; }
    public string Content { get; set; } = string.Empty;

    /// <summary>Sources cited by the RAG pipeline (JSON of product slugs / ids). Null for user turns.</summary>
    public string? Sources { get; set; }
}
