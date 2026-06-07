using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TurnoSys.Domain.Entities;

namespace TurnoSys.Infrastructure.Persistence.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(200);
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(500);
        builder.Property(u => u.NombreCompleto).IsRequired().HasMaxLength(200);
        builder.HasOne(u => u.Rol).WithMany(r => r.Usuarios).HasForeignKey(u => u.RolId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(u => u.Empresa).WithMany(e => e.Usuarios).HasForeignKey(u => u.EmpresaId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
    }
}
