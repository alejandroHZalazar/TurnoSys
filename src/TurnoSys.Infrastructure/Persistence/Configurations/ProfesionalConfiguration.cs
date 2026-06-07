using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TurnoSys.Domain.Entities;

namespace TurnoSys.Infrastructure.Persistence.Configurations;

public class ProfesionalConfiguration : IEntityTypeConfiguration<Profesional>
{
    public void Configure(EntityTypeBuilder<Profesional> builder)
    {
        builder.ToTable("Profesionales");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Apellido).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Email).HasMaxLength(200);
        builder.Property(p => p.ColorAgenda).IsRequired().HasMaxLength(7).HasDefaultValue("#4F46E5");
        builder.Ignore(p => p.NombreCompleto);
        builder.HasOne(p => p.Empresa).WithMany(e => e.Profesionales).HasForeignKey(p => p.EmpresaId).OnDelete(DeleteBehavior.Restrict);
    }
}
