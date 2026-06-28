using AutoMapper;
using ApolloSpoilers.Application.Common;
using ApolloSpoilers.Application.DTOs;
using ApolloSpoilers.Application.Interfaces;
using ApolloSpoilers.Application.Specifications;
using ApolloSpoilers.Domain.Entities;
using ApolloSpoilers.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ApolloSpoilers.Application.Services;

public class WishlistService : IWishlistService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ILogger<WishlistService> _logger;

    public WishlistService(IUnitOfWork uow, IMapper mapper, ILogger<WishlistService> logger)
    {
        _uow = uow;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<WishlistDto> GetAsync(Guid userId, CancellationToken ct = default)
    {
        var wishlist = await GetOrCreateAsync(userId, ct);
        return _mapper.Map<WishlistDto>(wishlist);
    }

    public async Task<Result<WishlistDto>> AddAsync(Guid userId, Guid productId, CancellationToken ct = default)
    {
        if (!await _uow.Repository<Product>().AnyAsync(p => p.Id == productId, ct))
            return Result.Failure<WishlistDto>("Product not found.", "NOT_FOUND");

        var wishlist = await GetOrCreateAsync(userId, ct);
        if (wishlist.Items.Any(i => i.ProductId == productId))
        {
            var existing = _mapper.Map<WishlistDto>(wishlist);
            return Result.Success(existing);
        }

        await _uow.Repository<WishlistItem>().AddAsync(new WishlistItem
        {
            WishlistId = wishlist.Id,
            ProductId = productId
        }, ct);
        await _uow.SaveChangesAsync(ct);

        wishlist = await GetOrCreateAsync(userId, ct);
        return Result.Success(_mapper.Map<WishlistDto>(wishlist));
    }

    public async Task<Result> RemoveAsync(Guid userId, Guid productId, CancellationToken ct = default)
    {
        var wishlist = await GetOrCreateAsync(userId, ct);
        var item = wishlist.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item is not null)
        {
            _uow.Repository<WishlistItem>().Remove(item);
            await _uow.SaveChangesAsync(ct);
        }
        return Result.Success();
    }

    private async Task<Wishlist> GetOrCreateAsync(Guid userId, CancellationToken ct)
    {
        var existing = (await _uow.Repository<Wishlist>().ListAsync(new WishlistByUserSpecification(userId), ct)).FirstOrDefault();
        if (existing is not null) return existing;

        var newWishlist = new Wishlist { UserId = userId };
        await _uow.Repository<Wishlist>().AddAsync(newWishlist, ct);
        await _uow.SaveChangesAsync(ct);
        return (await _uow.Repository<Wishlist>().ListAsync(new WishlistByUserSpecification(userId), ct)).First();
    }
}
