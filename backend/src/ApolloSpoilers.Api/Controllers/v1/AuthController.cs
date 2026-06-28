using Asp.Versioning;
using ApolloSpoilers.Api.Common;
using ApolloSpoilers.Application.Common;
using ApolloSpoilers.Application.DTOs;
using ApolloSpoilers.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApolloSpoilers.Api.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public class AuthController : ApiControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;
    private readonly ICurrentUserService _currentUser;

    public AuthController(IAuthService authService, IUserService userService, ICurrentUserService currentUser)
    {
        _authService = authService;
        _userService = userService;
        _currentUser = currentUser;
    }

    /// <summary>Register a new customer account.</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterRequestDto dto, CancellationToken ct)
        => ToActionResult(await _authService.RegisterAsync(dto, ct));

    /// <summary>Login with email + password.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto dto, CancellationToken ct)
        => ToActionResult(await _authService.LoginAsync(dto, ct));

    /// <summary>Exchange an expired access token + refresh token for a new pair.</summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponseDto>> Refresh([FromBody] RefreshRequestDto dto, CancellationToken ct)
        => ToActionResult(await _authService.RefreshAsync(dto.AccessToken, dto.RefreshToken, ct));

    /// <summary>Logout (invalidates refresh token).</summary>
    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult> Logout(CancellationToken ct)
    {
        if (_currentUser.UserId is null) return Unauthorized();
        return ToActionResult(await _authService.LogoutAsync(_currentUser.UserId.Value, ct));
    }

    /// <summary>Request a password reset email (token logged in dev).</summary>
    [HttpPost("forgot-password")]
    public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto dto, CancellationToken ct)
        => ToActionResult(await _authService.ForgotPasswordAsync(dto.Email, ct));

    /// <summary>Reset password using the token from email.</summary>
    [HttpPost("reset-password")]
    public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordRequestDto dto, CancellationToken ct)
        => ToActionResult(await _authService.ResetPasswordAsync(dto, ct));

    /// <summary>Get the current user's profile.</summary>
    [Authorize]
    [HttpGet("profile")]
    public async Task<ActionResult<UserProfileDto>> GetProfile(CancellationToken ct)
    {
        if (_currentUser.UserId is null) return Unauthorized();
        return ToActionResult(await _userService.GetProfileAsync(_currentUser.UserId.Value, ct));
    }

    /// <summary>Update the current user's profile.</summary>
    [Authorize]
    [HttpPut("profile")]
    public async Task<ActionResult<UserProfileDto>> UpdateProfile([FromBody] UpdateProfileRequestDto dto, CancellationToken ct)
    {
        if (_currentUser.UserId is null) return Unauthorized();
        return ToActionResult(await _userService.UpdateProfileAsync(_currentUser.UserId.Value, dto, ct));
    }
}
