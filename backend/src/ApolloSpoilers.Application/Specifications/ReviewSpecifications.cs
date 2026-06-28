using ApolloSpoilers.Domain.Entities;
using ApolloSpoilers.Domain.Specifications;

namespace ApolloSpoilers.Application.Specifications;

public class ReviewByProductSpecification : BaseSpecification<Review>
{
    public ReviewByProductSpecification(Guid productId)
    {
        AddInclude(r => r.User);
        Criteria = r => r.ProductId == productId && r.IsApproved;
        ApplyOrderByDescending(r => r.CreatedAt);
    }
}
