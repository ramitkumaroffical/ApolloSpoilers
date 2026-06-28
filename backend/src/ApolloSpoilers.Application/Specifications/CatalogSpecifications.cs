using ApolloSpoilers.Domain.Entities;
using ApolloSpoilers.Domain.Specifications;

namespace ApolloSpoilers.Application.Specifications;

public class ProductBySlugSpecification : BaseSpecification<Product>
{
    public ProductBySlugSpecification(string slug)
    {
        AddInclude(p => p.Images);
        AddInclude(p => p.Category);
        AddInclude(p => p.Inventory);
        AddInclude(p => p.Reviews);
        Criteria = p => p.Slug == slug && p.IsActive;
    }
}

public class ProductByIdSpecification : BaseSpecification<Product>
{
    public ProductByIdSpecification(Guid id)
    {
        AddInclude(p => p.Images);
        AddInclude(p => p.Category);
        AddInclude(p => p.Inventory);
        AddInclude(p => p.Reviews);
        Criteria = p => p.Id == id;
    }
}

public class ActiveProductBrandsSpecification : BaseSpecification<Product>
{
    public ActiveProductBrandsSpecification()
    {
        Criteria = p => p.IsActive && p.CarBrand != null && p.CarBrand != "";
        ApplyNoTracking();
    }
}

public class CarModelsSpecification : BaseSpecification<Product>
{
    public CarModelsSpecification(string carBrand)
    {
        Criteria = p => p.IsActive && p.CarBrand == carBrand && p.CarModel != null && p.CarModel != "";
        ApplyNoTracking();
    }
}

public class ProductByCategorySpecification : BaseSpecification<Product>
{
    public ProductByCategorySpecification(Guid categoryId)
    {
        AddInclude(p => p.Images);
        AddInclude(p => p.Category);
        AddInclude(p => p.Inventory);
        Criteria = p => p.CategoryId == categoryId && p.IsActive;
    }
}
