using AutoMapper;
using ApolloSpoilers.Application.Common;
using ApolloSpoilers.Application.DTOs;
using ApolloSpoilers.Application.Interfaces;
using ApolloSpoilers.Application.Specifications;
using ApolloSpoilers.Domain.Entities;
using ApolloSpoilers.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ApolloSpoilers.Application.Services;

public class CartService : ICartService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ILogger<CartService> _logger;

    public CartService(IUnitOfWork uow, IMapper mapper, ILogger<CartService> logger)
    {
        _uow = uow;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CartDto> GetAsync(Guid userId, CancellationToken ct = default)
    {
        var cart = await GetOrCreateCartAsync(userId, ct);
        return _mapper.Map<CartDto>(cart);
    }

    public async Task<Result<CartDto>> AddItemAsync(Guid userId, AddToCartDto dto, CancellationToken ct = default)
    {
        var productRepo = _uow.Repository<Product>();
        var product = (await productRepo.ListAsync(new ProductByIdSpecification(dto.ProductId), ct)).FirstOrDefault();
        if (product is null || !product.IsActive)
            return Result.Failure<CartDto>("Product not available.", "NOT_FOUND");

        if (product.Inventory is null || product.Inventory.StockQuantity < dto.Quantity)
            return Result.Failure<CartDto>("Insufficient stock.", "CONFLICT");

        var cart = await GetOrCreateCartAsync(userId, ct);
        var existing = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);
        if (existing is not null)
        {
            existing.Quantity += dto.Quantity;
            if (existing.Quantity > product.Inventory.StockQuantity)
                return Result.Failure<CartDto>("Cannot add more than available stock.", "CONFLICT");
            _uow.Repository<CartItem>().Update(existing);
        }
        else
        {
            var item = new CartItem
            {
                CartId = cart.Id,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity,
                UnitPrice = product.Price
            };
            await _uow.Repository<CartItem>().AddAsync(item, ct);
        }

        await _uow.SaveChangesAsync(ct);
        var refreshed = await GetOrCreateCartAsync(userId, ct);
        return Result.Success(_mapper.Map<CartDto>(refreshed));
    }

    public async Task<Result<CartDto>> UpdateItemAsync(Guid userId, Guid itemId, UpdateCartItemDto dto, CancellationToken ct = default)
    {
        var cart = await GetOrCreateCartAsync(userId, ct);
        var item = cart.Items.FirstOrDefault(i => i.Id == itemId);
        if (item is null)
            return Result.Failure<CartDto>("Cart item not found.", "NOT_FOUND");

        item.Quantity = dto.Quantity;
        _uow.Repository<CartItem>().Update(item);
        await _uow.SaveChangesAsync(ct);

        var refreshed = await GetOrCreateCartAsync(userId, ct);
        return Result.Success(_mapper.Map<CartDto>(refreshed));
    }

    public async Task<Result> RemoveItemAsync(Guid userId, Guid itemId, CancellationToken ct = default)
    {
        var cart = await GetOrCreateCartAsync(userId, ct);
        var item = cart.Items.FirstOrDefault(i => i.Id == itemId);
        if (item is null)
            return Result.Failure("Cart item not found.", "NOT_FOUND");

        _uow.Repository<CartItem>().Remove(item);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> ClearAsync(Guid userId, CancellationToken ct = default)
    {
        var cart = await GetOrCreateCartAsync(userId, ct);
        _uow.Repository<CartItem>().RemoveRange(cart.Items);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }

    private async Task<Cart> GetOrCreateCartAsync(Guid userId, CancellationToken ct)
    {
        var cartRepo = _uow.Repository<Cart>();
        var cart = await cartRepo.ListAsync(new CartByUserSpecification(userId), ct);
        var existing = cart.FirstOrDefault();
        if (existing is not null)
            return existing;

        var newCart = new Cart { UserId = userId };
        await cartRepo.AddAsync(newCart, ct);
        await _uow.SaveChangesAsync(ct);

        // Re-fetch with includes
        existing = (await cartRepo.ListAsync(new CartByUserSpecification(userId), ct)).First();
        return existing;
    }
}
