using ApolloSpoilers.Domain.Entities;
using ApolloSpoilers.Domain.Specifications;

namespace ApolloSpoilers.Application.Specifications;

public class CartByUserSpecification : BaseSpecification<Cart>
{
    public CartByUserSpecification(Guid userId)
    {
        AddInclude(c => c.Items);
        AddInclude("Items.Product");
        AddInclude("Items.Product.Inventory");
        AddInclude("Items.Product.Images");
        Criteria = c => c.UserId == userId;
    }
}

public class WishlistByUserSpecification : BaseSpecification<Wishlist>
{
    public WishlistByUserSpecification(Guid userId)
    {
        AddInclude(w => w.Items);
        AddInclude("Items.Product");
        Criteria = w => w.UserId == userId;
    }
}

public class OrderByIdSpecification : BaseSpecification<Order>
{
    public OrderByIdSpecification(Guid id)
    {
        AddInclude(o => o.Items);
        Criteria = o => o.Id == id;
    }
}

public class OrdersForUserSpecification : BaseSpecification<Order>
{
    public OrdersForUserSpecification(Guid userId, int page, int pageSize)
    {
        AddInclude(o => o.Items);
        Criteria = o => o.UserId == userId;
        ApplyOrderByDescending(o => o.CreatedAt);
        ApplyPaging((page - 1) * pageSize, pageSize);
    }
}

public class AllOrdersSpecification : BaseSpecification<Order>
{
    public AllOrdersSpecification(int page, int pageSize)
    {
        AddInclude(o => o.Items);
        ApplyOrderByDescending(o => o.CreatedAt);
        ApplyPaging((page - 1) * pageSize, pageSize);
    }
}
