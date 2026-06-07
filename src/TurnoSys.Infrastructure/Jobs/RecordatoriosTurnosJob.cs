using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TurnoSys.Domain.Enums;
using TurnoSys.Domain.Interfaces.Services;
using TurnoSys.Infrastructure.Persistence;

namespace TurnoSys.Infrastructure.Jobs;

public class RecordatoriosTurnosJob(
    ApplicationDbContext db,
    IEmailService emailService,
    ILogger<RecordatoriosTurnosJob> logger)
{
    public async Task Execute()
    {
        var diasAnticipacion = await ObtenerDiasAnticipacionAsync();
        var fechaObjetivo = DateTime.UtcNow.Date.AddDays(diasAnticipacion);

        var turnos = await db.Turnos
            .Include(t => t.Paciente)
            .Include(t => t.Profesional)
            .Include(t => t.Practica)
            .Where(t =>
                t.FechaHoraInicio.Date == fechaObjetivo
                && t.Estado == EstadoTurno.Reservado
                && !t.RecordatorioTurnoEnviado
                && !t.IsDeleted)
            .ToListAsync();

        logger.LogInformation("[RecordatoriosTurnos] Procesando {Count} turnos para {Fecha}", turnos.Count, fechaObjetivo);

        foreach (var turno in turnos)
        {
            try
            {
                var mensaje = new Domain.Interfaces.Services.EmailMessage(
                    To: turno.Paciente.Email ?? "",
                    ToName: turno.Paciente.NombreCompleto,
                    Subject: $"Recordatorio: Turno mañana — {turno.Practica.Nombre}",
                    HtmlBody: BuildRecordatorioHtml(turno.Paciente.NombreCompleto,
                        turno.Profesional.NombreCompleto,
                        turno.Practica.Nombre,
                        turno.FechaHoraInicio)
                );

                if (string.IsNullOrEmpty(turno.Paciente.Email))
                {
                    logger.LogWarning("[RecordatoriosTurnos] Paciente {Id} sin email, omitiendo.", turno.PacienteId);
                    turno.RecordatorioTurnoEnviado = true;
                }
                else
                {
                    var enviado = await emailService.EnviarAsync(mensaje);
                    turno.RecordatorioTurnoEnviado = enviado;

                    if (!enviado)
                        logger.LogWarning("[RecordatoriosTurnos] Falló envío para turno {Id}", turno.Id);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[RecordatoriosTurnos] Error procesando turno {Id}", turno.Id);
            }
        }

        await db.SaveChangesAsync();
        logger.LogInformation("[RecordatoriosTurnos] Job completado.");
    }

    private async Task<int> ObtenerDiasAnticipacionAsync()
    {
        var param = await db.ParametrosSistema
            .FirstOrDefaultAsync(p => p.IsGlobal && p.Clave == "RECORDATORIO_DIAS_ANTICIPACION");
        return int.TryParse(param?.Valor, out var dias) ? dias : 1;
    }

    private static string BuildRecordatorioHtml(string paciente, string profesional, string practica, DateTime fecha) =>
        $"""
        <div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;">
          <h2 style="color: #4F46E5;">Recordatorio de Turno</h2>
          <p>Hola <strong>{paciente}</strong>,</p>
          <p>Te recordamos que tenés un turno programado para <strong>mañana</strong>:</p>
          <table style="border-collapse: collapse; width: 100%;">
            <tr><td style="padding: 8px; border: 1px solid #e5e7eb;"><strong>Profesional</strong></td><td style="padding: 8px; border: 1px solid #e5e7eb;">{profesional}</td></tr>
            <tr><td style="padding: 8px; border: 1px solid #e5e7eb;"><strong>Práctica</strong></td><td style="padding: 8px; border: 1px solid #e5e7eb;">{practica}</td></tr>
            <tr><td style="padding: 8px; border: 1px solid #e5e7eb;"><strong>Fecha y hora</strong></td><td style="padding: 8px; border: 1px solid #e5e7eb;">{fecha:dd/MM/yyyy HH:mm}</td></tr>
          </table>
          <p style="margin-top: 20px; color: #6b7280;">Si no podés asistir, comunicate a la brevedad para reagendar.</p>
        </div>
        """;
}
