using ApolloSpoilers.Application.DTOs;
using ApolloSpoilers.Domain.Entities;
using AutoMapper;

namespace ApolloSpoilers.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Categories
        CreateMap<Category, CategoryDto>();
        CreateMap<ApplicationUser, UserProfileDto>();

        // Images
        CreateMap<ProductImage, ProductImageDto>();

        // Reviews
        CreateMap<Review, ReviewDto>()
            .ForMember(d => d.AuthorName, o => o.MapFrom(s => s.User != null ? s.User.FullName : "Anonymous"));

        // Products
        CreateMap<Product, ProductListItemDto>()
            .ForMember(d => d.PrimaryImageUrl, o => o.MapFrom<ProductPrimaryImageResolver>())
            .ForMember(d => d.StockQuantity, o => o.MapFrom(s => s.Inventory != null ? s.Inventory.StockQuantity : 0))
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category != null ? s.Category.Name : null));

        CreateMap<Product, ProductDetailDto>()
            .ForMember(d => d.StockQuantity, o => o.MapFrom(s => s.Inventory != null ? s.Inventory.StockQuantity : 0))
            .ForMember(d => d.LowStockThreshold, o => o.MapFrom(s => s.Inventory != null ? s.Inventory.LowStockThreshold : 0))
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category != null ? s.Category.Name : null));

        // Cart
        CreateMap<CartItem, CartItemDto>()
                .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product != null ? s.Product.Name : "(removed)"))
                .ForMember(d => d.ProductSlug, o => o.MapFrom(s => s.Product != null ? s.Product.Slug : null))
                .ForMember(d => d.PrimaryImageUrl, o => o.MapFrom<CartItemPrimaryImageResolver>())
                .ForMember(d => d.AvailableStock, o => o.MapFrom(s => s.Product != null && s.Product.Inventory != null
                    ? s.Product.Inventory.StockQuantity
                    : 0));
        CreateMap<Cart, CartDto>();

        // Wishlist
        CreateMap<WishlistItem, WishlistItemDto>()
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product != null ? s.Product.Name : "(removed)"))
            .ForMember(d => d.ProductSlug, o => o.MapFrom(s => s.Product != null ? s.Product.Slug : null))
            .ForMember(d => d.Price, o => o.MapFrom(s => s.Product != null ? s.Product.Price : 0))
            .ForMember(d => d.PrimaryImageUrl, o => o.MapFrom<WishlistItemPrimaryImageResolver>());
        CreateMap<Wishlist, WishlistDto>();

        // Orders
        CreateMap<OrderItem, OrderItemDto>();
        CreateMap<Order, OrderDto>();
    }
}

public sealed class ProductPrimaryImageResolver : IValueResolver<Product, ProductListItemDto, string?>
{
    public string? Resolve(Product source, ProductListItemDto destination, string? destMember, ResolutionContext context)
        => PrimaryImageResolver.Pick(source.Images);
}

public sealed class CartItemPrimaryImageResolver : IValueResolver<CartItem, CartItemDto, string?>
{
    public string? Resolve(CartItem source, CartItemDto destination, string? destMember, ResolutionContext context)
        => PrimaryImageResolver.Pick(source.Product?.Images);
}

internal sealed class WishlistItemPrimaryImageResolver : IValueResolver<WishlistItem, WishlistItemDto, string?>
{
    public string? Resolve(WishlistItem source, WishlistItemDto destination, string? destMember, ResolutionContext context)
        => PrimaryImageResolver.Pick(source.Product?.Images);
}

internal static class PrimaryImageResolver
{
    public static string? Pick(ICollection<ProductImage>? images)
    {
        if (images is null || images.Count == 0) return null;
        var ordered = images.OrderBy(i => i.DisplayOrder).ToList();
        return ordered.FirstOrDefault(i => i.IsPrimary)?.ImageUrl ?? ordered.First().ImageUrl;
    }
}
