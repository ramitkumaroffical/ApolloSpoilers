namespace ApolloSpoilers.Domain.Common;

/// <summary>
/// Base class for all domain entities. Provides audit fields and identity.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
