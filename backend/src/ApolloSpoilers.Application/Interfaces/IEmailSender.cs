namespace ApolloSpoilers.Application.Interfaces;

/// <summary>
/// Abstraction over email delivery so the application layer can request an
/// email without depending on a specific provider (SMTP, SendGrid, etc.).
/// </summary>
public interface IEmailSender
{
    /// <summary>Send an HTML email to a single recipient.</summary>
    Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken ct = default);
}
