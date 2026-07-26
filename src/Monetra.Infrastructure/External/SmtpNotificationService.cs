using Microsoft.Extensions.Logging;
using Monetra.Core.Interfaces;

namespace Monetra.Infrastructure.External;

public class SmtpNotificationService : INotificationService
{
    private readonly ILogger<SmtpNotificationService> _logger;

    public SmtpNotificationService(ILogger<SmtpNotificationService> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Enviando e-mail para {To}: {Subject}", to, subject);
        return Task.CompletedTask;
    }

    public Task SendVerificationEmailAsync(string to, string token, CancellationToken cancellationToken = default)
    {
        var link = $"http://localhost:5000/api/v1/auth/verify-email?token={token}";
        var body = $"Clique no link para verificar seu e-mail: {link}";
        return SendEmailAsync(to, "Verifique seu e-mail - Monetra", body, cancellationToken);
    }

    public Task SendPasswordResetEmailAsync(string to, string token, CancellationToken cancellationToken = default)
    {
        var link = $"http://localhost:5000/api/v1/auth/reset-password?token={token}";
        var body = $"Clique no link para redefinir sua senha: {link}";
        return SendEmailAsync(to, "Redefinição de senha - Monetra", body, cancellationToken);
    }
}
