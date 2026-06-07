using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TurnoSys.Domain.Entities;

namespace TurnoSys.Infrastructure.Persistence.Configurations;

public class ParametroSistemaConfiguration : IEntityTypeConfiguration<ParametroSistema>
{
    public void Configure(EntityTypeBuilder<ParametroSistema> builder)
    {
        builder.ToTable("ParametrosSistema");
        builder.HasKey(p => p.Id);

        // La tabla real solo tiene UpdatedAt (no CreatedAt ni IsDeleted de BaseEntity)
        builder.Ignore(p => p.CreatedAt);
        builder.Ignore(p => p.IsDeleted);

        builder.Property(p => p.Clave).IsRequired().HasMaxLength(100);
        builder.Property(p => p.TipoDato).IsRequired().HasMaxLength(20);
        builder.Property(p => p.Descripcion).HasMaxLength(500);
    }
}
