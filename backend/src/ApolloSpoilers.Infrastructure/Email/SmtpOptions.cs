namespace ApolloSpoilers.Infrastructure.Email;

/// <summary>
/// SMTP server configuration, bound from the "Email" section of appsettings.
/// </summary>
public class SmtpOptions
{
    public const string SectionName = "Email";

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 25;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string From { get; set; } = "no-reply@apollospoilers.com";
    public string FromName { get; set; } = "Apollo Spoilers";
    /// <summary>Use SSL/TLS (STARTTLS on port 587, implicit TLS on 465).</summary>
    public bool EnableSsl { get; set; } = true;
}
