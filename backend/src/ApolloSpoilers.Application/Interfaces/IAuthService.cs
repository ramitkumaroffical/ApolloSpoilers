using ApolloSpoilers.Application.Common;
using ApolloSpoilers.Application.DTOs;

namespace ApolloSpoilers.Application.Interfaces;

public interface IAuthService
{
    Task<Result<AuthResponseDto>> RegisterAsync(RegisterRequestDto dto, CancellationToken ct = default);
    Task<Result<AuthResponseDto>> LoginAsync(LoginRequestDto dto, CancellationToken ct = default);
    Task<Result<AuthResponseDto>> RefreshAsync(string accessToken, string refreshToken, CancellationToken ct = default);
    Task<Result> LogoutAsync(Guid userId, CancellationToken ct = default);
    Task<Result> ForgotPasswordAsync(string email, CancellationToken ct = default);
    Task<Result> ResetPasswordAsync(ResetPasswordRequestDto dto, CancellationToken ct = default);
}

public interface ICurrentUserService
{
    Guid? UserId { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
}

public interface IUserService
{
    Task<Result<UserProfileDto>> GetProfileAsync(Guid userId, CancellationToken ct = default);
    Task<Result<UserProfileDto>> UpdateProfileAsync(Guid userId, UpdateProfileRequestDto dto, CancellationToken ct = default);
}
