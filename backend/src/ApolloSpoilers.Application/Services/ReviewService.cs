using AutoMapper;
using ApolloSpoilers.Application.Common;
using ApolloSpoilers.Application.DTOs;
using ApolloSpoilers.Application.Interfaces;
using ApolloSpoilers.Application.Specifications;
using ApolloSpoilers.Domain.Entities;
using ApolloSpoilers.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ApolloSpoilers.Application.Services;

public class ReviewService : IReviewService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ILogger<ReviewService> _logger;

    public ReviewService(IUnitOfWork uow, IMapper mapper, ILogger<ReviewService> logger)
    {
        _uow = uow;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ReviewDto>> ListForProductAsync(Guid productId, CancellationToken ct = default)
    {
        var reviews = await _uow.Repository<Review>()
            .ListAsync(new ReviewByProductSpecification(productId), ct);
        return _mapper.Map<IReadOnlyList<ReviewDto>>(reviews);
    }

    public async Task<Result<ReviewDto>> AddReviewAsync(Guid productId, Guid userId, int rating, string? comment, CancellationToken ct = default)
    {
        if (rating < 1 || rating > 5)
            return Result.Failure<ReviewDto>("Rating must be between 1 and 5.", "VALIDATION");

        var productRepo = _uow.Repository<Product>();
        var product = await productRepo.GetByIdAsync(productId, ct);
        if (product is null)
            return Result.Failure<ReviewDto>("Product not found.", "NOT_FOUND");

        // Check for duplicate review
        var reviewRepo = _uow.Repository<Review>();
        var existing = (await reviewRepo.ListAsync(new ReviewByProductSpecification(productId), ct))
            .FirstOrDefault(r => r.UserId == userId);
        if (existing is not null)
            return Result.Failure<ReviewDto>("You have already reviewed this product.", "CONFLICT");

        var review = new Review
        {
            ProductId = productId,
            UserId = userId,
            Rating = rating,
            Comment = comment,
            IsApproved = false // Admin must approve
        };

        await reviewRepo.AddAsync(review, ct);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Review {ReviewId} added for product {ProductId}", review.Id, productId);

        return Result.Success(_mapper.Map<ReviewDto>(review));
    }

    public async Task<Result> ApproveReviewAsync(Guid reviewId, bool approve, CancellationToken ct = default)
    {
        var reviewRepo = _uow.Repository<Review>();
        var review = await reviewRepo.GetByIdAsync(reviewId, ct);
        if (review is null)
            return Result.Failure("Review not found.", "NOT_FOUND");

        review.IsApproved = approve;
        reviewRepo.Update(review);
        await _uow.SaveChangesAsync(ct);

        // Recalculate average rating on product
        var productRepo = _uow.Repository<Product>();
        var allReviews = await reviewRepo.ListAsync(new ReviewByProductSpecification(review.ProductId), ct);
        var approved = allReviews.Where(r => r.IsApproved).ToList();
        var product = await productRepo.GetByIdAsync(review.ProductId, ct);
        if (product is not null)
        {
            product.AverageRating = approved.Any() ? approved.Average(r => r.Rating) : 0;
            product.ReviewCount = approved.Count;
            product.UpdatedAt = DateTime.UtcNow;
            productRepo.Update(product);
            await _uow.SaveChangesAsync(ct);
        }

        return Result.Success();
    }
}
