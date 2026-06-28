using Asp.Versioning;
using ApolloSpoilers.Api.Common;
using ApolloSpoilers.Application.DTOs;
using ApolloSpoilers.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApolloSpoilers.Api.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/cart")]
[Authorize]
public class CartController : ApiControllerBase
{
    private readonly ICartService _cart;
    private readonly ICurrentUserService _currentUser;

    public CartController(ICartService cart, ICurrentUserService currentUser)
    {
        _cart = cart;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<CartDto>> Get(CancellationToken ct)
        => Ok(await _cart.GetAsync(_currentUser.UserId!.Value, ct));

    [HttpPost("items")]
    public async Task<ActionResult<CartDto>> AddItem([FromBody] AddToCartDto dto, CancellationToken ct)
        => ToActionResult(await _cart.AddItemAsync(_currentUser.UserId!.Value, dto, ct));

    [HttpPut("items/{itemId:guid}")]
    public async Task<ActionResult<CartDto>> UpdateItem(Guid itemId, [FromBody] UpdateCartItemDto dto, CancellationToken ct)
        => ToActionResult(await _cart.UpdateItemAsync(_currentUser.UserId!.Value, itemId, dto, ct));

    [HttpDelete("items/{itemId:guid}")]
    public async Task<ActionResult> RemoveItem(Guid itemId, CancellationToken ct)
        => ToActionResult(await _cart.RemoveItemAsync(_currentUser.UserId!.Value, itemId, ct));

    [HttpDelete]
    public async Task<ActionResult> Clear(CancellationToken ct)
        => ToActionResult(await _cart.ClearAsync(_currentUser.UserId!.Value, ct));
}
