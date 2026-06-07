using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TurnoSys.Domain.Enums;
using TurnoSys.Domain.Interfaces.Services;
using TurnoSys.Infrastructure.Persistence;

namespace TurnoSys.Infrastructure.Jobs;

public class RecordatoriosControlJob(
    ApplicationDbContext db,
    IEmailService emailService,
    ILogger<RecordatoriosControlJob> logger)
{
    public async Task Execute()
    {
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);

        var recordatorios = await db.RecordatoriosControl
            .Include(r => r.Profesional)
            .Include(r => r.Paciente)
            .Where(r => r.Estado == EstadoRecordatorio.Pendiente && r.FechaRecordatorioEnviar <= hoy)
            .ToListAsync();

        logger.LogInformation("[RecordatoriosControl] Procesando {Count} recordatorios de control.", recordatorios.Count);

        foreach (var rec in recordatorios)
        {
            try
            {
                // Si no hay email del profesional, no hay nada que reintentar.
                var emailOk = true;
                if (!string.IsNullOrEmpty(rec.Profesional.Email))
                {
                    var mensaje = new EmailMessage(
                        To: rec.Profesional.Email,
                        ToName: rec.Profesional.NombreCompleto,
                        Subject: $"Control pendiente — Paciente: {rec.Paciente.NombreCompleto}",
                        HtmlBody: BuildControlHtml(
                            rec.Profesional.NombreCompleto,
                            rec.Paciente.NombreCompleto,
                            rec.Paciente.Telefono,
                            rec.FechaControlSugerida)
                    );

                    emailOk = await emailService.EnviarAsync(mensaje);
                }

                // Notificación in-app al profesional
                var usuario = await db.Usuarios
                    .FirstOrDefaultAsync(u => u.Email == rec.Profesional.Email && u.IsActivo);

                if (usuario != null)
                {
                    await db.Notificaciones.AddAsync(new Domain.Entities.Notificacion
                    {
                        UsuarioId = usuario.Id,
                        Titulo = "Control de paciente pendiente",
                        Mensaje = $"{rec.Paciente.NombreCompleto} tiene un control sugerido para el {rec.FechaControlSugerida:dd/MM/yyyy}.",
                        Tipo = "recordatorio",
                        Url = $"/pacientes/{rec.PacienteId}"
                    });
                }

                rec.IntentoEnvio++;

                // Solo marcar Enviado si el correo salió bien; si falló, queda
                // Pendiente para reintentar en la próxima corrida del job.
                if (emailOk)
                {
                    rec.Estado = EstadoRecordatorio.Enviado;
                    rec.FechaEnvioReal = DateTime.UtcNow;
                }
                else
                {
                    logger.LogWarning(
                        "[RecordatoriosControl] Email no enviado para {Id}; queda pendiente para reintento.", rec.Id);
                }
            }
            catch (Exception ex)
            {
                rec.IntentoEnvio++;
                logger.LogError(ex, "[RecordatoriosControl] Error procesando recordatorio {Id}", rec.Id);
            }
        }

        await db.SaveChangesAsync();
        logger.LogInformation("[RecordatoriosControl] Job completado.");
    }

    private static string BuildControlHtml(string profesional, string paciente, string? telefono, DateOnly fecha)
    {
        var wa = BuildWhatsAppLink(telefono, paciente, fecha);

        var botonWhatsApp = wa is null
            ? "<p style=\"color:#9ca3af;font-size:13px;\">El paciente no tiene teléfono cargado.</p>"
            : $"""
              <a href="{wa}" target="_blank"
                 style="display:inline-block;background:#25D366;color:#ffffff;text-decoration:none;
                        font-weight:bold;padding:12px 20px;border-radius:8px;font-size:14px;">
                💬 Enviar WhatsApp al paciente
              </a>
              """;

        return $"""
        <div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;">
          <h2 style="color: #4F46E5;">Control de Paciente Pendiente</h2>
          <p>Hola <strong>{profesional}</strong>,</p>
          <p>Te recordamos que tu paciente <strong>{paciente}</strong> tiene un control sugerido para el <strong>{fecha:dd/MM/yyyy}</strong>.</p>
          <p>Podés contactarlo para coordinar el turno:</p>
          <p style="margin:20px 0;">{botonWhatsApp}</p>
        </div>
        """;
    }

    // Construye el link wa.me con el número del paciente y un mensaje prearmado.
    private static string? BuildWhatsAppLink(string? telefono, string paciente, DateOnly fecha)
    {
        if (string.IsNullOrWhiteSpace(telefono)) return null;

        // Dejar solo dígitos
        var soloDigitos = new string(telefono.Where(char.IsDigit).ToArray());
        if (soloDigitos.Length < 8) return null;

        // Si no trae código de país, anteponer 54 (Argentina)
        if (!soloDigitos.StartsWith("54"))
            soloDigitos = "54" + soloDigitos;

        var nombrePila = paciente.Contains(',')
            ? paciente.Split(',')[1].Trim()
            : paciente;

        var texto = Uri.EscapeDataString(
            $"Hola {nombrePila}, te contactamos para coordinar tu control del {fecha:dd/MM/yyyy}.");

        return $"https://wa.me/{soloDigitos}?text={texto}";
    }
}
