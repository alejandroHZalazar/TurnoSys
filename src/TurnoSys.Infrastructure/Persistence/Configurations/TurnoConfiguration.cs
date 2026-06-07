using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TurnoSys.Domain.Entities;
using TurnoSys.Domain.Enums;


namespace TurnoSys.Infrastructure.Persistence.Configurations;

public class TurnoConfiguration : IEntityTypeConfiguration<Turno>
{
    public void Configure(EntityTypeBuilder<Turno> builder)
    {
        builder.ToTable("Turnos");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Estado)
            .HasColumnName("EstadoId")
            .HasConversion<int>()
            .IsRequired()
            .HasSentinel(EstadoTurno.Disponible);

        builder.Property(t => t.Observaciones).HasMaxLength(2000);
        builder.Property(t => t.MotivoCancelacion).HasMaxLength(500);

        builder.HasOne(t => t.Empresa).WithMany().HasForeignKey(t => t.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(t => t.Profesional).WithMany(p => p.Turnos).HasForeignKey(t => t.ProfesionalId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(t => t.Paciente).WithMany(p => p.Turnos).HasForeignKey(t => t.PacienteId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(t => t.Practica).WithMany(p => p.Turnos).HasForeignKey(t => t.PracticaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(t => t.RecordatorioControl).WithOne(r => r.TurnoOrigen).HasForeignKey<RecordatorioControl>(r => r.TurnoOrigenId).IsRequired(false);

        builder.HasIndex(t => new { t.ProfesionalId, t.FechaHoraInicio, t.FechaHoraFin });
        builder.HasIndex(t => new { t.EmpresaId, t.FechaHoraInicio });
        builder.HasIndex(t => t.PacienteId);
    }
}
