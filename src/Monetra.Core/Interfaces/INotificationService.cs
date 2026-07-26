namespace Monetra.Core.Interfaces;

public interface INotificationService
{
    Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
    Task SendVerificationEmailAsync(string to, string token, CancellationToken cancellationToken = default);
    Task SendPasswordResetEmailAsync(string to, string token, CancellationToken cancellationToken = default);
}
