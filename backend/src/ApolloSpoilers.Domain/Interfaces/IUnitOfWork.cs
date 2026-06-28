namespace ApolloSpoilers.Domain.Interfaces;

/// <summary>
/// Unit of Work boundary. A single SaveChangesAsync call commits a transaction
/// spanning all repositories resolved from this UoW.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IRepository<T> Repository<T>() where T : Common.BaseEntity;
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
}
