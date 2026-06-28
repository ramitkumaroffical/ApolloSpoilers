using System.Net;
using System.Net.Mail;
using System.Text;
using ApolloSpoilers.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ApolloSpoilers.Infrastructure.Email;

/// <summary>
/// SMTP-backed implementation of <see cref="IEmailSender"/>. Uses the BCL
/// <see cref="SmtpClient"/> — no external NuGet dependency. Failures are
/// logged but never thrown to the caller, so a mail outage never breaks the
/// auth flow (the reset link is also written to the log).
/// </summary>
public class SmtpEmailSender : IEmailSender
{
    private readonly SmtpOptions _options;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<SmtpOptions> options, ILogger<SmtpEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        try
        {
            using var message = new MailMessage();
            message.From = string.IsNullOrWhiteSpace(_options.FromName)
                ? new MailAddress(_options.From)
                : new MailAddress(_options.From, _options.FromName, Encoding.UTF8);
            message.To.Add(to);
            message.Subject = subject;
            message.Body = htmlBody;
            message.IsBodyHtml = true;
            message.BodyEncoding = Encoding.UTF8;

            using var client = new SmtpClient(_options.Host, _options.Port);
            client.EnableSsl = _options.EnableSsl;
            if (!string.IsNullOrWhiteSpace(_options.Username))
            {
                client.Credentials = new NetworkCredential(_options.Username, _options.Password);
            }

            await client.SendMailAsync(message, ct);
            _logger.LogInformation("Email sent to {Recipient} (subject: {Subject})", to, subject);
        }
        catch (Exception ex)
        {
            // Don't throw — the reset link is also logged in AuthService, so
            // the flow still works in dev. In prod this surfaces as a silent
            // miss; investigate via the error log below.
            _logger.LogError(ex, "Failed to send email to {Recipient} (subject: {Subject})", to, subject);
        }
    }
}
