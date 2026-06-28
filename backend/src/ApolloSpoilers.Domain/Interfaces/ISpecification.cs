using System.Linq.Expressions;

namespace ApolloSpoilers.Domain.Interfaces;

/// <summary>
/// Generic specification abstraction used by repositories to compose
/// query criteria, ordering, and includes without leaking IQueryable.
/// </summary>
public interface ISpecification<T> where T : class
{
    Expression<Func<T, bool>>? Criteria { get; }
    List<Expression<Func<T, object>>> Includes { get; }
    List<string> IncludeStrings { get; }
    Expression<Func<T, object>>? OrderBy { get; }
    Expression<Func<T, object>>? OrderByDescending { get; }
    int? Take { get; }
    int? Skip { get; }
    bool AsNoTracking { get; }
}
