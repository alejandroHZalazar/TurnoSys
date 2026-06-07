using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TurnoSys.Domain.Entities;

namespace TurnoSys.Infrastructure.Persistence.Configurations;

public class ProfesionalPracticaConfiguration : IEntityTypeConfiguration<ProfesionalPractica>
{
    public void Configure(EntityTypeBuilder<ProfesionalPractica> builder)
    {
        builder.ToTable("ProfesionalesPracticas");
        builder.HasKey(pp => new { pp.ProfesionalId, pp.PracticaId });

        builder.HasOne(pp => pp.Profesional).WithMany(p => p.ProfesionalesPracticas).HasForeignKey(pp => pp.ProfesionalId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(pp => pp.Practica).WithMany(p => p.ProfesionalesPracticas).HasForeignKey(pp => pp.PracticaId).OnDelete(DeleteBehavior.Cascade);
    }
}
