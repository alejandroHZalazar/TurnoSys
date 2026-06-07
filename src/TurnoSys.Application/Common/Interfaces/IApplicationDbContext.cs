using Microsoft.EntityFrameworkCore;
using TurnoSys.Domain.Entities;

namespace TurnoSys.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Empresa> Empresas { get; }
    DbSet<Rol> Roles { get; }
    DbSet<Usuario> Usuarios { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Profesional> Profesionales { get; }
    DbSet<HorarioProfesional> HorariosProfesionales { get; }
    DbSet<BloqueoHorario> BloqueosHorarios { get; }
    DbSet<CategoriaPractica> CategoriasPracticas { get; }
    DbSet<Practica> Practicas { get; }
    DbSet<ProfesionalPractica> ProfesionalesPracticas { get; }
    DbSet<Paciente> Pacientes { get; }
    DbSet<Turno> Turnos { get; }
    DbSet<RecordatorioControl> RecordatoriosControl { get; }
    DbSet<ParametroSistema> ParametrosSistema { get; }
    DbSet<Notificacion> Notificaciones { get; }
    DbSet<AuditLog> AuditLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
