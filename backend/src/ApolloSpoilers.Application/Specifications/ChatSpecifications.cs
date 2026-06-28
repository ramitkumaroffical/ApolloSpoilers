using ApolloSpoilers.Domain.Entities;
using ApolloSpoilers.Domain.Specifications;

namespace ApolloSpoilers.Application.Specifications;

public class ChatSessionByIdSpecification : BaseSpecification<ChatSession>
{
    public ChatSessionByIdSpecification(Guid id)
    {
        AddInclude(s => s.Messages);
        Criteria = s => s.Id == id;
    }
}

public class RecentChatSessionsSpecification : BaseSpecification<ChatSession>
{
    public RecentChatSessionsSpecification(Guid userId, int take)
    {
        Criteria = s => s.UserId == userId;
        ApplyOrderByDescending(s => s.CreatedAt);
        Take = take;
    }
}
