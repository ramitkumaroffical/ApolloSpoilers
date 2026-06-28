using AutoMapper;
using ApolloSpoilers.Application.Common;
using ApolloSpoilers.Application.DTOs;
using ApolloSpoilers.Application.Interfaces;
using ApolloSpoilers.Application.Specifications;
using ApolloSpoilers.Domain.Entities;
using ApolloSpoilers.Domain.Enums;
using ApolloSpoilers.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ApolloSpoilers.Application.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IUnitOfWork uow, IMapper mapper, ILogger<OrderService> logger)
    {
        _uow = uow;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<OrderDto>> PlaceOrderAsync(Guid userId, CreateOrderDto dto, CancellationToken ct = default)
    {
        var cart = (await _uow.Repository<Cart>().ListAsync(new CartByUserSpecification(userId), ct)).FirstOrDefault();
        if (cart is null || !cart.Items.Any())
            return Result.Failure<OrderDto>("Cart is empty.", "VALIDATION");

        // Validate stock and snapshot product info
        foreach (var item in cart.Items)
        {
            var product = item.Product;
            if (product is null || !product.IsActive)
                return Result.Failure<OrderDto>($"Product '{item.ProductId}' is no longer available.", "CONFLICT");
            if (product.Inventory is null || product.Inventory.StockQuantity < item.Quantity)
                return Result.Failure<OrderDto>($"Insufficient stock for '{product.Name}'.", "CONFLICT");
        }

        var order = new Order
        {
            OrderNumber = await GenerateOrderNumberAsync(ct),
            UserId = userId,
            Subtotal = cart.Items.Sum(i => i.UnitPrice * i.Quantity),
            ShippingCost = 0, // Free shipping baseline; real carrier calc out of scope
            Status = OrderStatus.Pending,
            ShippingFullName = dto.ShippingFullName,
            ShippingAddressLine = dto.ShippingAddressLine,
            ShippingCity = dto.ShippingCity,
            ShippingState = dto.ShippingState,
            ShippingPostalCode = dto.ShippingPostalCode,
            ShippingCountry = dto.ShippingCountry,
            ShippingPhone = dto.ShippingPhone
        };

        foreach (var item in cart.Items)
        {
            order.Items.Add(new OrderItem
            {
                OrderId = order.Id,
                ProductId = item.ProductId,
                ProductName = item.Product!.Name,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            });

            // Decrement inventory
            item.Product!.Inventory!.StockQuantity -= item.Quantity;
            item.Product.Inventory.LastStockUpdate = DateTime.UtcNow;
            _uow.Repository<Inventory>().Update(item.Product.Inventory);
        }

        await _uow.Repository<Order>().AddAsync(order, ct);

        // Clear cart
        _uow.Repository<CartItem>().RemoveRange(cart.Items);

        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Order {OrderNumber} placed by user {UserId}", order.OrderNumber, userId);

        var created = (await _uow.Repository<Order>().ListAsync(new OrderByIdSpecification(order.Id), ct)).First();
        return Result.Success(_mapper.Map<OrderDto>(created));
    }

    public async Task<IReadOnlyList<OrderDto>> ListForUserAsync(Guid userId, CancellationToken ct = default)
    {
        var orders = await _uow.Repository<Order>().ListAsync(new OrdersForUserSpecification(userId, 1, 100), ct);
        return _mapper.Map<IReadOnlyList<OrderDto>>(orders);
    }

    public async Task<Result<OrderDto>> GetByIdAsync(Guid userId, Guid orderId, CancellationToken ct = default)
    {
        var order = (await _uow.Repository<Order>().ListAsync(new OrderByIdSpecification(orderId), ct)).FirstOrDefault();
        if (order is null || order.UserId != userId)
            return Result.Failure<OrderDto>("Order not found.", "NOT_FOUND");
        return Result.Success(_mapper.Map<OrderDto>(order));
    }

    public async Task<PagedResult<OrderDto>> ListAllAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var orderRepo = _uow.Repository<Order>();
        var spec = new AllOrdersSpecification(page, pageSize);
        var orders = await orderRepo.ListAsync(spec, ct);
        var total = await orderRepo.CountAsync(spec, ct);
        return new PagedResult<OrderDto>(_mapper.Map<IReadOnlyList<OrderDto>>(orders), total, page, pageSize);
    }

    public async Task<Result<OrderDto>> UpdateStatusAsync(Guid orderId, UpdateOrderStatusDto dto, CancellationToken ct = default)
    {
        var order = (await _uow.Repository<Order>().ListAsync(new OrderByIdSpecification(orderId), ct)).FirstOrDefault();
        if (order is null)
            return Result.Failure<OrderDto>("Order not found.", "NOT_FOUND");

        order.Status = dto.Status;
        order.UpdatedAt = DateTime.UtcNow;
        _uow.Repository<Order>().Update(order);
        await _uow.SaveChangesAsync(ct);

        var updated = (await _uow.Repository<Order>().ListAsync(new OrderByIdSpecification(orderId), ct)).First();
        return Result.Success(_mapper.Map<OrderDto>(updated));
    }

    private async Task<string> GenerateOrderNumberAsync(CancellationToken ct)
    {
        var year = DateTime.UtcNow.Year;
        var count = await _uow.Repository<Order>().CountAsync(new AllOrdersSpecification(1, 1), ct) + 1;
        // Use total count + 1 as the sequence — acceptable for low-volume stores
        var total = await _uow.Repository<Order>().CountAsync(ct: ct);
        return $"APS-{year}-{(total + 1):D6}";
    }
}
