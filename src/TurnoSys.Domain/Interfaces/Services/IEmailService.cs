namespace TurnoSys.Domain.Interfaces.Services;

public interface IEmailService
{
    Task<bool> EnviarAsync(EmailMessage mensaje, CancellationToken ct = default);
}

public record EmailMessage(
    string To,
    string ToName,
    string Subject,
    string HtmlBody,
    string? TextBody = null
);
