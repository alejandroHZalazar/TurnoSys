using TurnoSys.Domain.Entities.Common;

namespace TurnoSys.Domain.Entities;

public class CategoriaPractica : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Color { get; set; }
    public int Orden { get; set; } = 0;

    public Empresa Empresa { get; set; } = null!;
    public ICollection<Practica> Practicas { get; set; } = [];
}
