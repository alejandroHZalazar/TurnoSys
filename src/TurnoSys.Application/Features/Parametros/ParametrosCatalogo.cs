namespace TurnoSys.Application.Features.Parametros;

/// <summary>
/// Catálogo central de parámetros configurables del sistema.
/// Define metadata (sección, label, tipo, si es secreto y default).
/// </summary>
public static class ParametrosCatalogo
{
    public record ParametroMeta(
        string Clave,
        string Seccion,
        string Label,
        string Tipo,        // string | int | bool | time | dias | select
        string DefaultValor,
        bool EsSecreto = false,
        string? Opciones = null,   // para tipo select: "a,b,c"
        string? Ayuda = null
    );

    public static readonly IReadOnlyList<ParametroMeta> Todos =
    [
        // ── Turnos ──────────────────────────────────────────────
        new("TURNO_DURACION_MINUTOS", "Turnos", "Duración default (min)", "int", "30",
            Ayuda: "Duración por defecto de un turno cuando la práctica no la define."),
        new("TURNO_HORA_INICIO", "Turnos", "Hora de apertura", "time", "08:00"),
        new("TURNO_HORA_FIN", "Turnos", "Hora de cierre", "time", "20:00"),
        new("TURNO_DIAS_ATENCION", "Turnos", "Días de atención", "dias", "1,2,3,4,5",
            Ayuda: "Días hábiles por defecto de la agenda."),

        // ── Recordatorios ───────────────────────────────────────
        new("RECORDATORIO_DIAS_ANTICIPACION", "Recordatorios", "Días de anticipación", "int", "1",
            Ayuda: "Cuántos días antes del turno se envía el recordatorio."),
        new("RECORDATORIO_HORA_EJECUCION", "Recordatorios", "Hora de envío", "time", "08:00",
            Ayuda: "Hora a la que corre el job diario de recordatorios."),

        // ── Email ───────────────────────────────────────────────
        new("EMAIL_PROVEEDOR", "Email", "Proveedor", "select", "auto", Opciones: "auto,resend,smtp",
            Ayuda: "auto: usa Resend y cae a SMTP si falla."),
        new("EMAIL_FROM", "Email", "Email remitente", "string", ""),
        new("EMAIL_FROM_NAME", "Email", "Nombre remitente", "string", "TurnoSys"),
        new("EMAIL_TIMEOUT_SEGUNDOS", "Email", "Timeout (seg)", "int", "30"),
        new("EMAIL_RETRY_INTENTOS", "Email", "Reintentos", "int", "3"),
        new("EMAIL_RESEND_API_KEY", "Email", "Resend API Key", "string", "", EsSecreto: true),
        new("EMAIL_SMTP_HOST", "Email", "SMTP Host", "string", ""),
        new("EMAIL_SMTP_PORT", "Email", "SMTP Puerto", "int", "587"),
        new("EMAIL_SMTP_SSL", "Email", "SMTP SSL", "bool", "true"),
        new("EMAIL_SMTP_USERNAME", "Email", "SMTP Usuario", "string", ""),
        new("EMAIL_SMTP_PASSWORD", "Email", "SMTP Contraseña", "string", "", EsSecreto: true),
    ];

    public static ParametroMeta? Buscar(string clave) =>
        Todos.FirstOrDefault(p => p.Clave == clave);
}
