using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TurnoSys.Domain.Entities;

namespace TurnoSys.Infrastructure.Persistence.Seeders;

public class DatabaseSeeder(ApplicationDbContext db, ILogger<DatabaseSeeder> logger)
{
    public async Task SeedAsync()
    {
        // Verificar que la BD existe y las tablas están creadas antes de seedear.
        // Si no están, lanzar un mensaje claro en lugar de un error críptico.
        var puedeConectar = await db.Database.CanConnectAsync();
        if (!puedeConectar)
        {
            logger.LogError("[Seeder] No se puede conectar a la base de datos. Verificar cadena de conexión.");
            return;
        }

        var tablasExisten = await TablaExisteAsync("Roles");
        if (!tablasExisten)
        {
            logger.LogError("[Seeder] Las tablas no existen. Ejecutar primero los scripts SQL en database/01 al 05.");
            return;
        }

        await SeedRolesAsync();
        await SeedParametrosSistemaAsync();
        await SeedSuperAdminAsync();
        await db.SaveChangesAsync();

        logger.LogInformation("[Seeder] Completado.");
    }

    private async Task<bool> TablaExisteAsync(string tabla)
    {
        var conn = db.Database.GetDbConnection();
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText =
            $"SELECT COUNT(1) FROM INFORMATION_SCHEMA.TABLES " +
            $"WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = '{tabla}'";
        var result = await cmd.ExecuteScalarAsync();
        await conn.CloseAsync();
        return Convert.ToInt32(result) > 0;
    }

    private async Task SeedRolesAsync()
    {
        if (await db.Roles.AnyAsync()) return;

        db.Roles.AddRange(
            new Rol
            {
                Id = 1, Nombre = "SuperAdmin", Descripcion = "Administrador global del sistema",
                Permisos = null   // null = acceso total
            },
            new Rol
            {
                Id = 2, Nombre = "Administrador", Descripcion = "Administrador de empresa",
                Permisos = """["agenda.ver","agenda.crear","agenda.editar","agenda.cancelar","pacientes.ver","pacientes.crear","pacientes.editar","pacientes.eliminar","profesionales.ver","profesionales.crear","profesionales.editar","profesionales.eliminar","practicas.ver","practicas.crear","practicas.editar","practicas.eliminar","dashboard.ver","empresa.ver","empresa.editar","configuracion.ver","configuracion.editar","usuarios.ver","usuarios.crear","usuarios.editar","usuarios.desactivar"]"""
            },
            new Rol
            {
                Id = 3, Nombre = "Recepcionista", Descripcion = "Gestión de turnos y pacientes",
                Permisos = """["agenda.ver","agenda.crear","agenda.editar","agenda.cancelar","pacientes.ver","pacientes.crear","pacientes.editar","profesionales.ver","practicas.ver","dashboard.ver"]"""
            },
            new Rol
            {
                Id = 4, Nombre = "Profesional", Descripcion = "Acceso a su propia agenda",
                Permisos = """["agenda.ver","pacientes.ver","dashboard.ver"]"""
            }
        );

        logger.LogInformation("[Seeder] Roles creados.");
    }

    private async Task SeedParametrosSistemaAsync()
    {
        if (await db.ParametrosSistema.AnyAsync()) return;

        db.ParametrosSistema.AddRange(
            new ParametroSistema { Clave = "TURNO_DURACION_MINUTOS",         Valor = "30",    TipoDato = "int",    Descripcion = "Duración default de turno en minutos",                IsGlobal = true },
            new ParametroSistema { Clave = "RECORDATORIO_DIAS_ANTICIPACION", Valor = "1",     TipoDato = "int",    Descripcion = "Días antes del turno para enviar recordatorio",        IsGlobal = true },
            new ParametroSistema { Clave = "RECORDATORIO_HORA_EJECUCION",    Valor = "08:00", TipoDato = "string", Descripcion = "Hora del job de recordatorios (HH:mm)",                IsGlobal = true },
            new ParametroSistema { Clave = "EMAIL_PROVEEDOR",                Valor = "auto",  TipoDato = "string", Descripcion = "Proveedor email: resend|smtp|auto",                    IsGlobal = true },
            new ParametroSistema { Clave = "EMAIL_FROM",                     Valor = "",      TipoDato = "string", Descripcion = "Email remitente",                                      IsGlobal = true },
            new ParametroSistema { Clave = "EMAIL_FROM_NAME",                Valor = "TurnoSys", TipoDato = "string", Descripcion = "Nombre remitente",                                 IsGlobal = true },
            new ParametroSistema { Clave = "EMAIL_TIMEOUT_SEGUNDOS",         Valor = "30",    TipoDato = "int",    Descripcion = "Timeout de envío de email",                           IsGlobal = true },
            new ParametroSistema { Clave = "EMAIL_RETRY_INTENTOS",           Valor = "3",     TipoDato = "int",    Descripcion = "Reintentos en caso de falla",                          IsGlobal = true },
            new ParametroSistema { Clave = "TURNO_HORA_INICIO",              Valor = "08:00", TipoDato = "string", Descripcion = "Hora inicio atención default",                         IsGlobal = true },
            new ParametroSistema { Clave = "TURNO_HORA_FIN",                 Valor = "20:00", TipoDato = "string", Descripcion = "Hora fin atención default",                            IsGlobal = true },
            new ParametroSistema { Clave = "LOGIN_MAX_INTENTOS_FALLIDOS",    Valor = "5",     TipoDato = "int",    Descripcion = "Intentos fallidos antes de bloqueo",                   IsGlobal = true },
            new ParametroSistema { Clave = "LOGIN_BLOQUEO_MINUTOS_BASE",     Valor = "15",    TipoDato = "int",    Descripcion = "Minutos base de bloqueo por intentos fallidos",         IsGlobal = true }
        );

        logger.LogInformation("[Seeder] Parámetros del sistema creados.");
    }

    private async Task SeedSuperAdminAsync()
    {
        if (await db.Usuarios.AnyAsync(u => u.Email == "admin@turnosys.com")) return;

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Admin1234!", workFactor: 12);

        db.Usuarios.Add(new Usuario
        {
            Email          = "admin@turnosys.com",
            PasswordHash   = passwordHash,
            NombreCompleto = "Administrador del Sistema",
            RolId          = 1,
            IsActivo       = true
        });

        logger.LogInformation("[Seeder] SuperAdmin creado: admin@turnosys.com / Admin1234!");
    }
}
