using ApolloSpoilers.Application.Common;
using ApolloSpoilers.Application.DTOs;

namespace ApolloSpoilers.Application.Interfaces;

public interface ICartService
{
    Task<CartDto> GetAsync(Guid userId, CancellationToken ct = default);
    Task<Result<CartDto>> AddItemAsync(Guid userId, AddToCartDto dto, CancellationToken ct = default);
    Task<Result<CartDto>> UpdateItemAsync(Guid userId, Guid itemId, UpdateCartItemDto dto, CancellationToken ct = default);
    Task<Result> RemoveItemAsync(Guid userId, Guid itemId, CancellationToken ct = default);
    Task<Result> ClearAsync(Guid userId, CancellationToken ct = default);
}

public interface IWishlistService
{
    Task<WishlistDto> GetAsync(Guid userId, CancellationToken ct = default);
    Task<Result<WishlistDto>> AddAsync(Guid userId, Guid productId, CancellationToken ct = default);
    Task<Result> RemoveAsync(Guid userId, Guid productId, CancellationToken ct = default);
}

public interface IOrderService
{
    Task<Result<OrderDto>> PlaceOrderAsync(Guid userId, CreateOrderDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<OrderDto>> ListForUserAsync(Guid userId, CancellationToken ct = default);
    Task<Result<OrderDto>> GetByIdAsync(Guid userId, Guid orderId, CancellationToken ct = default);
    Task<PagedResult<OrderDto>> ListAllAsync(int page, int pageSize, CancellationToken ct = default);
    Task<Result<OrderDto>> UpdateStatusAsync(Guid orderId, UpdateOrderStatusDto dto, CancellationToken ct = default);
}
