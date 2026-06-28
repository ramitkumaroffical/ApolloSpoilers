using System.Linq.Expressions;
using ApolloSpoilers.Domain.Common;

namespace ApolloSpoilers.Domain.Interfaces;

/// <summary>Generic repository contract. Read & write surfaces over an aggregate root.</summary>
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> ListAsync(ISpecification<T>? spec = null, CancellationToken ct = default);
    Task<int> CountAsync(ISpecification<T>? spec = null, CancellationToken ct = default);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

    Task<T> AddAsync(T entity, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);
}
