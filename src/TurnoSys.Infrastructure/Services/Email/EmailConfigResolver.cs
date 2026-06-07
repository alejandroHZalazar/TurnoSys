using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TurnoSys.Infrastructure.Persistence;

namespace TurnoSys.Infrastructure.Services.Email;

/// <summary>
/// Resuelve cada valor de configuración de email siguiendo el orden:
///   1) Variable de entorno
///   2) Parámetro en base de datos (ParametrosSistema, prioriza override de empresa)
///   3) appsettings.json
/// Cachea la lectura de BD por scope (una sola query por envío).
/// </summary>
public class EmailConfigResolver(ApplicationDbContext db, IConfiguration config)
{
    private static readonly string[] ClavesEmail =
    [
        "EMAIL_PROVEEDOR", "EMAIL_FROM", "EMAIL_FROM_NAME",
        "EMAIL_TIMEOUT_SEGUNDOS", "EMAIL_RETRY_INTENTOS", "EMAIL_RESEND_API_KEY",
        "EMAIL_SMTP_HOST", "EMAIL_SMTP_PORT", "EMAIL_SMTP_SSL",
        "EMAIL_SMTP_USERNAME", "EMAIL_SMTP_PASSWORD",
    ];

    private Dictionary<string, string?>? _bdCache;

    public async Task<string?> GetAsync(string? envVar, string claveBd, string? appKey, CancellationToken ct = default)
    {
        // 1) Variable de entorno
        if (!string.IsNullOrEmpty(envVar))
        {
            var ev = Environment.GetEnvironmentVariable(envVar);
            if (!string.IsNullOrEmpty(ev)) return ev;
        }

        // 2) Base de datos
        _bdCache ??= await CargarBdAsync(ct);
        if (_bdCache.TryGetValue(claveBd, out var v) && !string.IsNullOrEmpty(v)) return v;

        // 3) appsettings
        if (!string.IsNullOrEmpty(appKey))
        {
            var c = config[appKey];
            if (!string.IsNullOrEmpty(c)) return c;
        }

        return null;
    }

    private async Task<Dictionary<string, string?>> CargarBdAsync(CancellationToken ct)
    {
        var rows = await db.ParametrosSistema
            .AsNoTracking()
            .Where(p => ClavesEmail.Contains(p.Clave))
            .OrderByDescending(p => p.EmpresaId != null)   // override de empresa antes que global
            .ThenByDescending(p => p.UpdatedAt)
            .Select(p => new { p.Clave, p.Valor })
            .ToListAsync(ct);

        return rows
            .GroupBy(p => p.Clave)
            .ToDictionary(g => g.Key, g => g.First().Valor);
    }
}
