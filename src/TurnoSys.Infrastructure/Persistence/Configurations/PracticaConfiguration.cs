using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TurnoSys.Domain.Entities;

namespace TurnoSys.Infrastructure.Persistence.Configurations;

public class PracticaConfiguration : IEntityTypeConfiguration<Practica>
{
    public void Configure(EntityTypeBuilder<Practica> builder)
    {
        builder.ToTable("Practicas");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Nombre).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Precio).HasPrecision(18, 2).HasDefaultValue(0m);
        builder.Property(p => p.Color).HasMaxLength(7);
        builder.HasOne(p => p.Empresa).WithMany(e => e.Practicas).HasForeignKey(p => p.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.Categoria).WithMany(c => c.Practicas).HasForeignKey(p => p.CategoriaId).IsRequired(false).OnDelete(DeleteBehavior.SetNull);
    }
}
