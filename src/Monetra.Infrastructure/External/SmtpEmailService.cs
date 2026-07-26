using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Monetra.Infrastructure.External;

/// <summary>
/// Serviço de envio de emails via SMTP.
/// </summary>
public class SmtpEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Envia um email.
    /// </summary>
    public async Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        try
        {
            var host = _configuration["Smtp:Host"] ?? "localhost";
            var port = int.Parse(_configuration["Smtp:Port"] ?? "1025");
            var username = _configuration["Smtp:Username"];
            var password = _configuration["Smtp:Password"];
            var fromEmail = _configuration["Smtp:FromEmail"] ?? "noreply@monetra.com.br";
            var fromName = _configuration["Smtp:FromName"] ?? "Monetra";

            using var client = new SmtpClient(host, port);

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                client.Credentials = new NetworkCredential(username, password);
                client.EnableSsl = true;
            }

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(to);

            await client.SendMailAsync(mailMessage, cancellationToken);
            _logger.LogInformation("Email enviado para {To}: {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar email para {To}: {Subject}", to, subject);
            // Não lançar exceção - email é não-crítico
        }
    }

    /// <summary>
    /// Envia email de verificação.
    /// </summary>
    public async Task SendVerificationEmailAsync(string to, string name, string verificationToken, CancellationToken cancellationToken = default)
    {
        var subject = "Monetra - Verifique seu email";
        var body = $@"
            <h2>Bem-vindo ao Monetra, {name}!</h2>
            <p>Obrigado por se cadastrar. Para ativar sua conta, clique no link abaixo:</p>
            <p><a href='http://localhost:5000/api/v1/auth/verify-email?token={verificationToken}'>Verificar Email</a></p>
            <p>Se você não se cadastrou no Monetra, ignore este email.</p>
            <br/>
            <p>Equipe Monetra</p>";

        await SendAsync(to, subject, body, cancellationToken);
    }

    /// <summary>
    /// Envia email de recuperação de senha.
    /// </summary>
    public async Task SendPasswordResetEmailAsync(string to, string name, string resetToken, CancellationToken cancellationToken = default)
    {
        var subject = "Monetra - Recuperação de Senha";
        var body = $@"
            <h2>Recuperação de Senha</h2>
            <p>Olá, {name}!</p>
            <p>Recebemos uma solicitação para redefinir sua senha. Clique no link abaixo:</p>
            <p><a href='http://localhost:3000/reset-password?token={resetToken}'>Redefinir Senha</a></p>
            <p>Este link expira em 1 hora.</p>
            <p>Se você não solicitou a redefinição de senha, ignore este email.</p>
            <br/>
            <p>Equipe Monetra</p>";

        await SendAsync(to, subject, body, cancellationToken);
    }
}
