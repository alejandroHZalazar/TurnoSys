namespace TurnoSys.Application.Common.Interfaces;

/// <summary>
/// Programa (o reprograma) los jobs recurrentes de recordatorios
/// leyendo la hora de ejecución configurada en ParametrosSistema.
/// Implementado en Infrastructure con Hangfire.
/// </summary>
public interface IRecordatorioScheduler
{
    Task ProgramarAsync(CancellationToken ct = default);
}
