namespace ApolloSpoilers.Application.DTOs;

public class ChatMessageDto
{
    public Guid Id { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class SendMessageDto
{
    public Guid? SessionId { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class ChatResponseDto
{
    public Guid SessionId { get; set; }
    public string Answer { get; set; } = string.Empty;
    public IReadOnlyList<ChatSourceDto> Sources { get; set; } = Array.Empty<ChatSourceDto>();
}

public class ChatSourceDto
{
    public string Type { get; set; } = string.Empty;   // "product" | "category" | "policy"
    public Guid? ProductId { get; set; }
    public string? ProductSlug { get; set; }
    public string? ProductName { get; set; }
    public double Score { get; set; }
}
