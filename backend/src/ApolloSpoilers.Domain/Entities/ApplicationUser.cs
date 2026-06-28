using Microsoft.AspNetCore.Identity;

namespace ApolloSpoilers.Domain.Entities;

/// <summary>
/// Extended Identity user for Apollo Spoilers. Adds profile fields plus
/// the rotating refresh token used by the JWT auth flow.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    /// <summary>Current refresh token (hashed at rest in Infrastructure layer).</summary>
    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiry { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;

    public string FullName => $"{FirstName} {LastName}".Trim();
}
