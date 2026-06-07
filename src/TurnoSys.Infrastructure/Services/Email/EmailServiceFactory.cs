using Microsoft.Extensions.Logging;
using TurnoSys.Domain.Interfaces.Services;

namespace TurnoSys.Infrastructure.Services.Email;

public class EmailServiceFactory(
    EmailConfigResolver resolver,
    ILogger<EmailServiceFactory> logger,
    ResendEmailService resend,
    SmtpEmailService smtp) : IEmailService
{
    public async Task<bool> EnviarAsync(EmailMessage mensaje, CancellationToken ct = default)
    {
        var proveedor = (await resolver.GetAsync(null, "EMAIL_PROVEEDOR", null, ct) ?? "auto").ToLower();

        var tieneResend = !string.IsNullOrEmpty(
            await resolver.GetAsync("RESEND_API_KEY", "EMAIL_RESEND_API_KEY", "Email:ResendApiKey", ct));
        var tieneSmtp = !string.IsNullOrEmpty(
            await resolver.GetAsync("SMTP_HOST", "EMAIL_SMTP_HOST", "Email:Smtp:Host", ct));

        // Resolver proveedor efectivo
        var usarResend = proveedor == "resend" || (proveedor == "auto" && tieneResend);
        var usarSmtp   = proveedor == "smtp"   || (proveedor == "auto" && !tieneResend && tieneSmtp);

        if (usarResend)
        {
            var ok = await resend.EnviarAsync(mensaje, ct);
            if (ok) return true;

            logger.LogWarning("Resend falló. {Fallback}",
                tieneSmtp ? "Intentando fallback SMTP." : "No hay SMTP configurado como fallback.");
            return tieneSmtp && await smtp.EnviarAsync(mensaje, ct);
        }

        if (usarSmtp)
            return await smtp.EnviarAsync(mensaje, ct);

        logger.LogError("Ningún proveedor de email está configurado (revisar /configuracion → Email).");
        return false;
    }
}
