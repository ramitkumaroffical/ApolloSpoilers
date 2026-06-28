using System.Linq.Expressions;
using ApolloSpoilers.Application.DTOs;
using ApolloSpoilers.Domain.Entities;
using ApolloSpoilers.Domain.Specifications;

namespace ApolloSpoilers.Application.Specifications;

/// <summary>
/// Builds a queryable filter expression for products from a <see cref="ProductQueryDto"/>.
/// Public storefront listing only returns active products.
/// </summary>
public class ProductFilterSpecification : BaseSpecification<Product>
{
    public ProductFilterSpecification(ProductQueryDto query, bool includeInactive = false)
    {
        AddInclude(p => p.Images);
        AddInclude(p => p.Category);
        AddInclude(p => p.Inventory);

        if (!includeInactive)
            Criteria = p => p.IsActive;

        ApplyFilters(query);
        ApplySort(query.SortBy);
        ApplyPaging((query.Page - 1) * query.PageSize, query.PageSize);
    }

    private void ApplyFilters(ProductQueryDto query)
    {
        var existing = Criteria;

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            Expression<Func<Product, bool>> search = p =>
                p.Name.Contains(term) || p.Description.Contains(term) ||
                (p.CarBrand != null && p.CarBrand.Contains(term)) ||
                (p.CarModel != null && p.CarModel.Contains(term));
            Criteria = existing == null ? search : And(existing, search);
        }
        if (query.CategoryId.HasValue)
        {
            Expression<Func<Product, bool>> cat = p => p.CategoryId == query.CategoryId.Value;
            Criteria = Criteria == null ? cat : And(Criteria, cat);
        }
        if (!string.IsNullOrWhiteSpace(query.CarBrand))
        {
            var b = query.CarBrand.Trim();
            Expression<Func<Product, bool>> brand = p => p.CarBrand == b;
            Criteria = Criteria == null ? brand : And(Criteria, brand);
        }
        if (!string.IsNullOrWhiteSpace(query.CarModel))
        {
            var m = query.CarModel.Trim();
            Expression<Func<Product, bool>> model = p => p.CarModel == m;
            Criteria = Criteria == null ? model : And(Criteria, model);
        }
        if (query.MinPrice.HasValue)
        {
            var min = query.MinPrice.Value;
            Expression<Func<Product, bool>> priceMin = p => p.Price >= min;
            Criteria = Criteria == null ? priceMin : And(Criteria, priceMin);
        }
        if (query.MaxPrice.HasValue)
        {
            var max = query.MaxPrice.Value;
            Expression<Func<Product, bool>> priceMax = p => p.Price <= max;
            Criteria = Criteria == null ? priceMax : And(Criteria, priceMax);
        }
        if (query.IsFeatured.HasValue)
        {
            var f = query.IsFeatured.Value;
            Expression<Func<Product, bool>> feat = p => p.IsFeatured == f;
            Criteria = Criteria == null ? feat : And(Criteria, feat);
        }
    }

    private void ApplySort(ProductSortOption sort)
    {
        switch (sort)
        {
            case ProductSortOption.PriceAscending:
                ApplyOrderBy(p => p.Price); break;
            case ProductSortOption.PriceDescending:
                ApplyOrderByDescending(p => p.Price); break;
            case ProductSortOption.Rating:
                ApplyOrderByDescending(p => p.AverageRating); break;
            case ProductSortOption.NameAscending:
                ApplyOrderBy(p => p.Name); break;
            case ProductSortOption.Newest:
            default:
                ApplyOrderByDescending(p => p.CreatedAt); break;
        }
    }

    // Combine two predicates with AndAlso.
    private static Expression<Func<T, bool>> And<T>(Expression<Func<T, bool>> a, Expression<Func<T, bool>> b)
    {
        var param = Expression.Parameter(typeof(T), "x");
        var body = Expression.AndAlso(
            Expression.Invoke(a, param),
            Expression.Invoke(b, param));
        return Expression.Lambda<Func<T, bool>>(body, param);
    }
}
