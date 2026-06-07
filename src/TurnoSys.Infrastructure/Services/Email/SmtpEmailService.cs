using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using TurnoSys.Domain.Interfaces.Services;

namespace TurnoSys.Infrastructure.Services.Email;

public class SmtpEmailService(
    EmailConfigResolver resolver,
    ILogger<SmtpEmailService> logger) : IEmailService
{
    public async Task<bool> EnviarAsync(EmailMessage mensaje, CancellationToken ct = default)
    {
        try
        {
            var host = await resolver.GetAsync("SMTP_HOST", "EMAIL_SMTP_HOST", "Email:Smtp:Host", ct);
            var port = int.Parse(await resolver.GetAsync("SMTP_PORT", "EMAIL_SMTP_PORT", "Email:Smtp:Port", ct) ?? "587");
            var useSsl = ParseBool(await resolver.GetAsync("SMTP_SSL", "EMAIL_SMTP_SSL", "Email:Smtp:UseSsl", ct), true);
            var username = await resolver.GetAsync("SMTP_USERNAME", "EMAIL_SMTP_USERNAME", "Email:Smtp:Username", ct);
            var password = await resolver.GetAsync("SMTP_PASSWORD", "EMAIL_SMTP_PASSWORD", "Email:Smtp:Password", ct);
            var from = await resolver.GetAsync("SMTP_FROM", "EMAIL_FROM", "Email:From", ct);
            var fromName = await resolver.GetAsync("SMTP_FROM_NAME", "EMAIL_FROM_NAME", "Email:FromName", ct) ?? "TurnoSys";

            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(from))
            {
                logger.LogWarning("[SMTP] Configuración incompleta (host o remitente sin definir).");
                return false;
            }

            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress(fromName, from));
            mimeMessage.To.Add(new MailboxAddress(mensaje.ToName, mensaje.To));
            mimeMessage.Subject = mensaje.Subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = mensaje.HtmlBody,
                TextBody = mensaje.TextBody
            };
            mimeMessage.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            var socketOptions = useSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTlsWhenAvailable;
            await client.ConnectAsync(host, port, socketOptions, ct);
            await client.AuthenticateAsync(username, password, ct);
            await client.SendAsync(mimeMessage, ct);
            await client.DisconnectAsync(true, ct);

            logger.LogInformation("[SMTP] Email enviado a {To}: {Subject}", mensaje.To, mensaje.Subject);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[SMTP] Error enviando email a {To}", mensaje.To);
            return false;
        }
    }

    // Acepta "true"/"false" y también "1"/"0" (compatibilidad con otras apps)
    private static bool ParseBool(string? value, bool def)
    {
        if (string.IsNullOrWhiteSpace(value)) return def;
        if (value == "1") return true;
        if (value == "0") return false;
        return bool.TryParse(value, out var b) ? b : def;
    }
}
