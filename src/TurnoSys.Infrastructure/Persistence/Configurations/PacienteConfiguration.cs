using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TurnoSys.Domain.Entities;

namespace TurnoSys.Infrastructure.Persistence.Configurations;

public class PacienteConfiguration : IEntityTypeConfiguration<Paciente>
{
    public void Configure(EntityTypeBuilder<Paciente> builder)
    {
        builder.ToTable("Pacientes");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Apellido).IsRequired().HasMaxLength(100);
        builder.Property(p => p.DNI).HasMaxLength(20);
        builder.Property(p => p.Email).HasMaxLength(200);
        // Las columnas reales en la BD usan nombres abreviados
        builder.Property(p => p.ContactoEmergenciaNombre).HasColumnName("ContactoEmergNombre");
        builder.Property(p => p.ContactoEmergenciaTelefono).HasColumnName("ContactoEmergTelefono");
        builder.Ignore(p => p.NombreCompleto);
        builder.HasIndex(p => new { p.EmpresaId, p.DNI });
        builder.HasIndex(p => new { p.EmpresaId, p.Apellido, p.Nombre });
        builder.HasOne(p => p.Empresa).WithMany(e => e.Pacientes).HasForeignKey(p => p.EmpresaId).OnDelete(DeleteBehavior.Restrict);
    }
}
