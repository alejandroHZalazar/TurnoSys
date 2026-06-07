using Microsoft.EntityFrameworkCore;
using TurnoSys.Application.Common.Interfaces;
using TurnoSys.Domain.Entities;
using TurnoSys.Domain.Interfaces.Services;

namespace TurnoSys.Infrastructure.Persistence;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    ICurrentUserService currentUser) : DbContext(options), IApplicationDbContext
{
    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Profesional> Profesionales => Set<Profesional>();
    public DbSet<HorarioProfesional> HorariosProfesionales => Set<HorarioProfesional>();
    public DbSet<BloqueoHorario> BloqueosHorarios => Set<BloqueoHorario>();
    public DbSet<CategoriaPractica> CategoriasPracticas => Set<CategoriaPractica>();
    public DbSet<Practica> Practicas => Set<Practica>();
    public DbSet<ProfesionalPractica> ProfesionalesPracticas => Set<ProfesionalPractica>();
    public DbSet<Paciente> Pacientes => Set<Paciente>();
    public DbSet<Turno> Turnos => Set<Turno>();
    public DbSet<RecordatorioControl> RecordatoriosControl => Set<RecordatorioControl>();
    public DbSet<ParametroSistema> ParametrosSistema => Set<ParametroSistema>();
    public DbSet<Notificacion> Notificaciones => Set<Notificacion>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Roles: IDs fijos (1-4), no autogenerados por la BD
        modelBuilder.Entity<Rol>().Property(r => r.Id).ValueGeneratedNever();

        // Global query filter: soft delete para entidades con IsDeleted
        modelBuilder.Entity<Empresa>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Profesional>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Paciente>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Practica>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Turno>().HasQueryFilter(e => !e.IsDeleted);

        // Filtros equivalentes en entidades hijas (consistencia de soft-delete)
        modelBuilder.Entity<HorarioProfesional>().HasQueryFilter(h => !h.Profesional.IsDeleted);
        modelBuilder.Entity<BloqueoHorario>().HasQueryFilter(b => !b.Profesional.IsDeleted);
        modelBuilder.Entity<CategoriaPractica>().HasQueryFilter(c => !c.Empresa.IsDeleted);
        modelBuilder.Entity<RecordatorioControl>().HasQueryFilter(r => !r.Paciente.IsDeleted);
        modelBuilder.Entity<ProfesionalPractica>().HasQueryFilter(pp => !pp.Practica.IsDeleted);

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        foreach (var entry in ChangeTracker.Entries<Domain.Entities.Common.AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedBy = currentUser.UserId;

            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedBy = currentUser.UserId;
            }
        }

        foreach (var entry in ChangeTracker.Entries<Domain.Entities.Common.BaseEntity>())
        {
            if (entry.State is EntityState.Added or EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
        }

        return await base.SaveChangesAsync(ct);
    }
}
