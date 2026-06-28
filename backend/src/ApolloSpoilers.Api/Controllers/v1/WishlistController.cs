using Asp.Versioning;
using ApolloSpoilers.Api.Common;
using ApolloSpoilers.Application.DTOs;
using ApolloSpoilers.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApolloSpoilers.Api.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/wishlist")]
[Authorize]
public class WishlistController : ApiControllerBase
{
    private readonly IWishlistService _wishlist;
    private readonly ICurrentUserService _currentUser;

    public WishlistController(IWishlistService wishlist, ICurrentUserService currentUser)
    {
        _wishlist = wishlist;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<WishlistDto>> Get(CancellationToken ct)
        => Ok(await _wishlist.GetAsync(_currentUser.UserId!.Value, ct));

    [HttpPost("{productId:guid}")]
    public async Task<ActionResult<WishlistDto>> Add(Guid productId, CancellationToken ct)
        => ToActionResult(await _wishlist.AddAsync(_currentUser.UserId!.Value, productId, ct));

    [HttpDelete("{productId:guid}")]
    public async Task<ActionResult> Remove(Guid productId, CancellationToken ct)
        => ToActionResult(await _wishlist.RemoveAsync(_currentUser.UserId!.Value, productId, ct));
}
