using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TurnoSys.Application.Common.Interfaces;
using TurnoSys.Infrastructure.Persistence;

namespace TurnoSys.Infrastructure.Jobs;

public class HangfireRecordatorioScheduler(
    ApplicationDbContext db,
    ILogger<HangfireRecordatorioScheduler> logger) : IRecordatorioScheduler
{
    public async Task ProgramarAsync(CancellationToken ct = default)
    {
        var hora = await ObtenerHoraEjecucionAsync(ct);
        var cron = Cron.Daily(hora.Hour, hora.Minute);
        var tz   = ResolverTimeZoneArgentina();

        var options = new RecurringJobOptions { TimeZone = tz };

        RecurringJob.AddOrUpdate<RecordatoriosTurnosJob>(
            "recordatorios-turnos", j => j.Execute(), cron, options);

        RecurringJob.AddOrUpdate<RecordatoriosControlJob>(
            "recordatorios-control", j => j.Execute(), cron, options);

        logger.LogInformation(
            "[Scheduler] Recordatorios programados a las {Hora} ({Tz}).",
            $"{hora.Hour:D2}:{hora.Minute:D2}", tz.Id);
    }

    private async Task<TimeOnly> ObtenerHoraEjecucionAsync(CancellationToken ct)
    {
        // Prioriza un override de empresa sobre el valor global; toma el más reciente.
        var param = await db.ParametrosSistema
            .AsNoTracking()
            .Where(p => p.Clave == "RECORDATORIO_HORA_EJECUCION")
            .OrderByDescending(p => p.EmpresaId != null)
            .ThenByDescending(p => p.UpdatedAt)
            .FirstOrDefaultAsync(ct);

        return TimeOnly.TryParse(param?.Valor, out var h) ? h : new TimeOnly(8, 0);
    }

    private static TimeZoneInfo ResolverTimeZoneArgentina()
    {
        // IANA (Linux/macOS y .NET 6+ en Windows) y fallback al id de Windows
        foreach (var id in new[] { "America/Argentina/Buenos_Aires", "Argentina Standard Time" })
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
            catch (TimeZoneNotFoundException) { }
            catch (InvalidTimeZoneException) { }
        }
        return TimeZoneInfo.Utc;
    }
}
