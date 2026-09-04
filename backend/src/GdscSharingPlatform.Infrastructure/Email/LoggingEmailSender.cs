using GdscSharingPlatform.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace GdscSharingPlatform.Infrastructure.Email;

public sealed class LoggingEmailSender : IEmailSender
{
    private readonly ILogger<LoggingEmailSender> _logger;

    public LoggingEmailSender(ILogger<LoggingEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendEmailChangeConfirmationAsync(
        string email,
        string confirmationLink,
        string token,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[EMAIL CONFIRMATION] Sending email change token to {Email}. Token: {Token}. Link: {ConfirmationLink}",
            email,
            token,
            confirmationLink);

        return Task.CompletedTask;
    }
}
