using Asp.Versioning;
using ApolloSpoilers.Api.Common;
using ApolloSpoilers.Application.DTOs;
using ApolloSpoilers.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApolloSpoilers.Api.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/orders")]
[Authorize]
public class OrdersController : ApiControllerBase
{
    private readonly IOrderService _orders;
    private readonly ICurrentUserService _currentUser;

    public OrdersController(IOrderService orders, ICurrentUserService currentUser)
    {
        _orders = orders;
        _currentUser = currentUser;
    }

    /// <summary>Place a new order from current cart (no payment).</summary>
    [HttpPost]
    public async Task<ActionResult<OrderDto>> PlaceOrder([FromBody] CreateOrderDto dto, CancellationToken ct)
        => ToActionResult(await _orders.PlaceOrderAsync(_currentUser.UserId!.Value, dto, ct));

    /// <summary>Order history for the current user.</summary>
    [HttpGet("my")]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> MyOrders(CancellationToken ct)
        => Ok(await _orders.ListForUserAsync(_currentUser.UserId!.Value, ct));

    /// <summary>Single order detail (must belong to caller unless admin).</summary>
    [HttpGet("{orderId:guid}")]
    public async Task<ActionResult<OrderDto>> GetById(Guid orderId, CancellationToken ct)
        => ToActionResult(await _orders.GetByIdAsync(_currentUser.UserId!.Value, orderId, ct));
}

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/orders")]
[Authorize(Roles = "Admin")]
public class AdminOrdersController : ApiControllerBase
{
    private readonly IOrderService _orders;

    public AdminOrdersController(IOrderService orders) => _orders = orders;

    [HttpGet]
    public async Task<ActionResult> ListAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        => Ok(await _orders.ListAllAsync(page, pageSize, ct));

    [HttpPut("{orderId:guid}/status")]
    public async Task<ActionResult<OrderDto>> UpdateStatus(Guid orderId, [FromBody] UpdateOrderStatusDto dto, CancellationToken ct)
        => ToActionResult(await _orders.UpdateStatusAsync(orderId, dto, ct));
}
