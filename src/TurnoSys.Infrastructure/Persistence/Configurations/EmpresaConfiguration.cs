using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TurnoSys.Domain.Entities;

namespace TurnoSys.Infrastructure.Persistence.Configurations;

public class EmpresaConfiguration : IEntityTypeConfiguration<Empresa>
{
    public void Configure(EntityTypeBuilder<Empresa> builder)
    {
        builder.ToTable("Empresas");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.RazonSocial).IsRequired().HasMaxLength(200);
        builder.Property(e => e.NombreFantasia).HasMaxLength(200);
        builder.Property(e => e.CUIT).HasMaxLength(20);
        builder.Property(e => e.Email).HasMaxLength(200);
        builder.Property(e => e.TimeZone).IsRequired().HasMaxLength(100).HasDefaultValue("America/Argentina/Buenos_Aires");

        // Logotipo almacenado como BLOB en la BD (MySQL LONGBLOB)
        builder.Property(e => e.LogotipoBlob).HasColumnType("longblob");
        builder.Property(e => e.LogotipoContentType).HasMaxLength(100);
    }
}
