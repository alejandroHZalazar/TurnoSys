using MediatR;
using TurnoSys.Application.Common.Models;
using TurnoSys.Domain.Interfaces.Services;

namespace TurnoSys.Application.Features.Parametros.Commands.ProbarEmail;

public class ProbarEmailCommandHandler(IEmailService emailService)
    : IRequestHandler<ProbarEmailCommand, Result>
{
    public async Task<Result> Handle(ProbarEmailCommand request, CancellationToken ct)
    {
        var mensaje = new EmailMessage(
            To: request.Destinatario,
            ToName: request.Destinatario,
            Subject: "TurnoSys — Email de prueba",
            HtmlBody: """
                <div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;">
                  <h2 style="color: #4F46E5;">Configuración de email correcta ✅</h2>
                  <p>Este es un correo de prueba enviado desde la configuración de TurnoSys.</p>
                  <p>Si lo recibiste, el envío de notificaciones está funcionando correctamente.</p>
                </div>
                """,
            TextBody: "Email de prueba de TurnoSys. La configuración funciona correctamente."
        );

        var enviado = await emailService.EnviarAsync(mensaje, ct);

        return enviado
            ? Result.Ok()
            : Result.Fail("No se pudo enviar el email. Revisá la configuración del proveedor (Resend/SMTP).");
    }
}
