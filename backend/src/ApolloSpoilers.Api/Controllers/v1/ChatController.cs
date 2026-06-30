using ApolloSpoilers.Application.DTOs;
using ApolloSpoilers.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ApolloSpoilers.Api.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IAasraChatService _chatService;

    public ChatController(IAasraChatService chatService)
    {
        _chatService = chatService;
    }

    [HttpPost]
    public async Task<ActionResult<ChatResponseDto>> Send(
        [FromBody] SendMessageDto dto,
        CancellationToken ct)
    {
        var userId = GetUserId();

        var result = await _chatService.SendMessageAsync(userId, dto, ct);

        if (result.Value.SessionId == Guid.Empty &&
            string.Equals(result.Value.Answer, "Chat session not found.", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(new
            {
                message = result.Value.Answer
            });
        }

        if (result.Value.SessionId == Guid.Empty &&
            string.Equals(result.Value.Answer, "Message cannot be empty.", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                message = result.Value.Answer
            });
        }

        return Ok(result.Value);
    }

    private Guid? GetUserId()
    {
        var userIdClaim = User?.FindFirst("sub")?.Value
            ?? User?.FindFirst("userId")?.Value
            ?? User?.FindFirst("id")?.Value;

        return Guid.TryParse(userIdClaim, out var id) ? id : null;
    }
}