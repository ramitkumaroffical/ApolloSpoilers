using System.Net;
using System.Security.Claims;
using ApolloSpoilers.Application.Common;
using ApolloSpoilers.Application.DTOs;
using ApolloSpoilers.Application.Interfaces;
using ApolloSpoilers.Domain.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ApolloSpoilers.Infrastructure.Identity;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly IJwtTokenService _jwtService;
    private readonly IEmailSender _emailSender;
    private readonly IMapper _mapper;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        IJwtTokenService jwtService,
        IEmailSender emailSender,
        IMapper mapper,
        IConfiguration config,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtService = jwtService;
        _emailSender = emailSender;
        _mapper = mapper;
        _config = config;
        _logger = logger;
    }

    public async Task<Result<AuthResponseDto>> RegisterAsync(RegisterRequestDto dto, CancellationToken ct = default)
    {
        var existing = await _userManager.FindByEmailAsync(dto.Email);
        if (existing is not null)
            return Result.Failure<AuthResponseDto>("Email already registered.", "CONFLICT");

        var user = new ApplicationUser
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            UserName = dto.Email,
            PhoneNumber = dto.PhoneNumber
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return Result.Failure<AuthResponseDto>(string.Join(" | ", result.Errors.Select(e => e.Description)), "VALIDATION");

        await _userManager.AddToRoleAsync(user, "Customer");

        var roles = await _userManager.GetRolesAsync(user);
        var refreshToken = _jwtService.GenerateRefreshToken();
        var refreshExpiry = DateTime.UtcNow.AddDays(int.Parse(_config["Jwt:RefreshTokenExpirationDays"] ?? "7"));

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = refreshExpiry;
        await _userManager.UpdateAsync(user);

        var accessToken = _jwtService.GenerateAccessToken(user, roles.ToList());

        _logger.LogInformation("User registered: {Email}", dto.Email);

        return Result.Success(new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            RefreshTokenExpiry = refreshExpiry,
            User = _mapper.Map<UserProfileDto>(user) with { Roles = roles.ToList() }
        });
    }

    public async Task<Result<AuthResponseDto>> LoginAsync(LoginRequestDto dto, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            return Result.Failure<AuthResponseDto>("Invalid credentials.", "UNAUTHORIZED");

        if (!user.IsActive)
            return Result.Failure<AuthResponseDto>("Account is disabled.", "FORBIDDEN");

        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        var refreshToken = _jwtService.GenerateRefreshToken();
        var refreshExpiry = DateTime.UtcNow.AddDays(int.Parse(_config["Jwt:RefreshTokenExpirationDays"] ?? "7"));

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = refreshExpiry;
        await _userManager.UpdateAsync(user);

        var accessToken = _jwtService.GenerateAccessToken(user, roles.ToList());

        return Result.Success(new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            RefreshTokenExpiry = refreshExpiry,
            User = _mapper.Map<UserProfileDto>(user) with { Roles = roles.ToList() }
        });
    }

    public async Task<Result<AuthResponseDto>> RefreshAsync(string accessToken, string refreshToken, CancellationToken ct = default)
    {
        var principal = _jwtService.GetPrincipalFromExpiredToken(accessToken);
        if (principal is null)
            return Result.Failure<AuthResponseDto>("Invalid token.", "UNAUTHORIZED");

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
            return Result.Failure<AuthResponseDto>("Invalid token claims.", "UNAUTHORIZED");

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpiry <= DateTime.UtcNow)
            return Result.Failure<AuthResponseDto>("Refresh token expired or invalid.", "UNAUTHORIZED");

        var roles = await _userManager.GetRolesAsync(user);
        var newRefreshToken = _jwtService.GenerateRefreshToken();
        var refreshExpiry = DateTime.UtcNow.AddDays(int.Parse(_config["Jwt:RefreshTokenExpirationDays"] ?? "7"));

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiry = refreshExpiry;
        await _userManager.UpdateAsync(user);

        var newAccessToken = _jwtService.GenerateAccessToken(user, roles.ToList());

        return Result.Success(new AuthResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            RefreshTokenExpiry = refreshExpiry,
            User = _mapper.Map<UserProfileDto>(user) with { Roles = roles.ToList() }
        });
    }

    public async Task<Result> LogoutAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return Result.Failure("User not found.", "NOT_FOUND");

        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        await _userManager.UpdateAsync(user);
        return Result.Success();
    }

    public async Task<Result> ForgotPasswordAsync(string email, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            // Don't reveal whether email exists — just return success.
            return Result.Success();
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        // The reset token can contain '+', '/', '=' and spaces — URL-encode it
        // so it survives the trip through the email link and query string.
        var encodedToken = Uri.EscapeDataString(token);
        var encodedEmail = Uri.EscapeDataString(email);
        var frontendUrl = _config["App:FrontendUrl"]?.TrimEnd('/') ?? "http://localhost:4200";
        var resetLink = $"{frontendUrl}/reset-password?email={encodedEmail}&token={encodedToken}";

        // Always log the link so the flow is testable even without SMTP.
        _logger.LogInformation("Password reset link for {Email}: {ResetLink}", email, resetLink);

        var subject = "Reset your Apollo Spoilers password";
        var body = BuildResetEmailHtml(user.FirstName, resetLink);
        await _emailSender.SendEmailAsync(email, subject, body, ct);

        return Result.Success();
    }

    public async Task<Result> ResetPasswordAsync(ResetPasswordRequestDto dto, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
        {
            // Don't reveal whether the email exists — mirror ForgotPassword's
            // behaviour to prevent account enumeration.
            return Result.Success();
        }

        var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
        if (!result.Succeeded)
            return Result.Failure(string.Join(" | ", result.Errors.Select(e => e.Description)), "VALIDATION");

        _logger.LogInformation("Password reset for {Email}", dto.Email);
        return Result.Success();
    }

    private static string BuildResetEmailHtml(string firstName, string resetLink)
    {
        var name = string.IsNullOrWhiteSpace(firstName) ? "there" : firstName;
        return $@"<!DOCTYPE html>
<html lang=""en""><body style=""font-family:Arial,Helvetica,sans-serif;background:#0c1118;color:#e8eaed;margin:0;padding:24px;"">
  <div style=""max-width:520px;margin:0 auto;background:#12151b;border:1px solid rgba(255,176,32,0.24);border-radius:16px;overflow:hidden;"">
    <div style=""height:5px;background:linear-gradient(90deg,#ff3d1f,#ffb020);""></div>
    <div style=""padding:32px;"">
      <h1 style=""margin:0 0 12px;font-size:22px;color:#ffffff;"">Reset your password</h1>
      <p style=""margin:0 0 16px;line-height:1.6;color:#a0a8b0;"">Hi {WebUtility.HtmlEncode(name)},</p>
      <p style=""margin:0 0 24px;line-height:1.6;color:#a0a8b0;"">We received a request to reset the password for your Apollo Spoilers account. This link is valid for 2 hours.</p>
      <p style=""margin:0 0 24px;"">
        <a href=""{resetLink}"" style=""display:inline-block;padding:14px 28px;border-radius:10px;background:linear-gradient(135deg,#ff3d1f,#9f1d12);color:#ffffff;text-decoration:none;font-weight:700;"">Reset password</a>
      </p>
      <p style=""margin:0 0 8px;line-height:1.6;color:#a0a8b0;"">If the button doesn't work, copy and paste this link into your browser:</p>
      <p style=""margin:0 0 24px;word-break:break-all;font-size:13px;color:#ffb020;"">{WebUtility.HtmlEncode(resetLink)}</p>
      <hr style=""border:none;border-top:1px solid rgba(255,255,255,0.08);margin:24px 0;"">
      <p style=""margin:0;line-height:1.6;color:#6b7480;font-size:13px;"">If you didn't request this, you can safely ignore this email — your password stays unchanged.</p>
    </div>
  </div>
</body></html>";
    }
}
