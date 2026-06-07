using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using TurnoSys.Domain.Interfaces.Services;

namespace TurnoSys.Infrastructure.Services.Email;

public class ResendEmailService(
    IHttpClientFactory httpClientFactory,
    EmailConfigResolver resolver,
    ILogger<ResendEmailService> logger) : IEmailService
{
    private readonly AsyncRetryPolicy _retryPolicy = Policy
        .Handle<HttpRequestException>()
        .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
            (ex, delay, attempt, _) =>
                Console.WriteLine($"[Resend] Intento {attempt} fallido, reintentando en {delay.TotalSeconds}s: {ex.Message}"));

    public async Task<bool> EnviarAsync(EmailMessage mensaje, CancellationToken ct = default)
    {
        var apiKey = await resolver.GetAsync("RESEND_API_KEY", "EMAIL_RESEND_API_KEY", "Email:ResendApiKey", ct);
        if (string.IsNullOrEmpty(apiKey))
        {
            logger.LogWarning("[Resend] API Key no configurada.");
            return false;
        }

        var fromEmail = await resolver.GetAsync("SMTP_FROM", "EMAIL_FROM", "Email:From", ct);
        var fromName  = await resolver.GetAsync("SMTP_FROM_NAME", "EMAIL_FROM_NAME", "Email:FromName", ct) ?? "TurnoSys";
        if (string.IsNullOrEmpty(fromEmail))
        {
            logger.LogWarning("[Resend] Email remitente no configurado.");
            return false;
        }
        var from = $"{fromName} <{fromEmail}>";

        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var client = httpClientFactory.CreateClient("Resend");
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var payload = new
            {
                from,
                to = new[] { $"{mensaje.ToName} <{mensaje.To}>" },
                subject = mensaje.Subject,
                html = mensaje.HtmlBody,
                text = mensaje.TextBody
            };

            var response = await client.PostAsJsonAsync("https://api.resend.com/emails", payload, ct);

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("[Resend] Email enviado a {To}: {Subject}", mensaje.To, mensaje.Subject);
                return true;
            }

            var error = await response.Content.ReadAsStringAsync(ct);
            logger.LogError("[Resend] Error {StatusCode}: {Error}", response.StatusCode, error);
            return false;
        });
    }
}
