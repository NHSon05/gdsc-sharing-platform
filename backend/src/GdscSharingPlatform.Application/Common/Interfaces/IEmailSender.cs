namespace GdscSharingPlatform.Application.Common.Interfaces;

public interface IEmailSender
{
    Task SendEmailChangeConfirmationAsync(
        string email,
        string confirmationLink,
        string token,
        CancellationToken cancellationToken = default);
}
