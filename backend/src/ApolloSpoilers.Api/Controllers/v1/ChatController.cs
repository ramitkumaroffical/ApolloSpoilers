using Asp.Versioning;
using ApolloSpoilers.Api.Common;
using ApolloSpoilers.Application.DTOs;
using ApolloSpoilers.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApolloSpoilers.Api.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/chat")]
public class ChatController : ApiControllerBase
{
    private readonly IAasraChatService _aasra;
    private readonly ICurrentUserService _currentUser;

    public ChatController(IAasraChatService aasra, ICurrentUserService currentUser)
    {
        _aasra = aasra;
        _currentUser = currentUser;
    }

    /// <summary>Send a message to Aasra, the AI shopping assistant.</summary>
    /// <remarks>Anonymous (guest) use is allowed — the conversation is not persisted for guests.</remarks>
    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<ChatResponseDto>> Send([FromBody] SendMessageDto dto, CancellationToken ct)
        => ToActionResult(await _aasra.SendMessageAsync(_currentUser.UserId, dto, ct));

    /// <summary>Get conversation history for a chat session.</summary>
    [HttpGet("{sessionId:guid}/history")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<ChatMessageDto>>> History(Guid sessionId, CancellationToken ct)
        => Ok(await _aasra.GetHistoryAsync(sessionId, ct));
}
