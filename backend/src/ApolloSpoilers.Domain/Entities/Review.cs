using ApolloSpoilers.Domain.Common;

namespace ApolloSpoilers.Domain.Entities;

public class Review : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }

    public Guid UserId { get; set; }
    public ApplicationUser? User { get; set; }

    /// <summary>1 to 5 stars.</summary>
    public int Rating { get; set; }
    public string? Comment { get; set; }

    public bool IsApproved { get; set; } = false;
}
