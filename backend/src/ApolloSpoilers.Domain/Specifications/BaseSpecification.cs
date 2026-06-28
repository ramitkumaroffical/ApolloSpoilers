using System.Linq.Expressions;
using ApolloSpoilers.Domain.Interfaces;

namespace ApolloSpoilers.Domain.Specifications;

/// <summary>
/// Reusable base implementation of <see cref="ISpecification{T}"/>. Application
/// layer derives concrete query specs from this to keep filter logic declarative
/// and isolated from EF/IQueryable concerns.
/// </summary>
public abstract class BaseSpecification<T> : ISpecification<T> where T : class
{
    public Expression<Func<T, bool>>? Criteria { get; protected set; }
    public List<Expression<Func<T, object>>> Includes { get; } = new();
    public List<string> IncludeStrings { get; } = new();
    public Expression<Func<T, object>>? OrderBy { get; protected set; }
    public Expression<Func<T, object>>? OrderByDescending { get; protected set; }
    public int? Take { get; protected set; }
    public int? Skip { get; protected set; }
    public bool AsNoTracking { get; protected set; }

    protected void AddInclude(Expression<Func<T, object>> include) => Includes.Add(include);
    protected void AddInclude(string includeString) => IncludeStrings.Add(includeString);
    protected void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
    }
    protected void ApplyOrderBy(Expression<Func<T, object>> expr) => OrderBy = expr;
    protected void ApplyOrderByDescending(Expression<Func<T, object>> expr) => OrderByDescending = expr;
    protected void ApplyNoTracking() => AsNoTracking = true;
}
