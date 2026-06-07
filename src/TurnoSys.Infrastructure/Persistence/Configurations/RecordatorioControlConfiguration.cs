using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TurnoSys.Domain.Entities;

namespace TurnoSys.Infrastructure.Persistence.Configurations;

public class RecordatorioControlConfiguration : IEntityTypeConfiguration<RecordatorioControl>
{
    public void Configure(EntityTypeBuilder<RecordatorioControl> builder)
    {
        builder.ToTable("RecordatoriosControl");
        builder.HasKey(r => r.Id);

        // La tabla solo tiene CreatedAt (no UpdatedAt ni IsDeleted de BaseEntity)
        builder.Ignore(r => r.UpdatedAt);
        builder.Ignore(r => r.IsDeleted);

        // El enum se guarda como texto ('Pendiente'/'Enviado'/'Cancelado'),
        // no como int — la columna es VARCHAR con CHECK constraint.
        builder.Property(r => r.Estado).HasConversion<string>().HasMaxLength(20);
    }
}
