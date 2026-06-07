using TurnoSys.Domain.Entities.Common;

namespace TurnoSys.Domain.Entities;

public class Practica : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Guid? CategoriaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; } = 0;
    public int DuracionMinutos { get; set; } = 30;
    public string? Color { get; set; }
    public bool RequiereObservaciones { get; set; } = false;
    public int? RecordatorioRecDias { get; set; }
    public bool IsActivo { get; set; } = true;

    public Empresa Empresa { get; set; } = null!;
    public CategoriaPractica? Categoria { get; set; }
    public ICollection<ProfesionalPractica> ProfesionalesPracticas { get; set; } = [];
    public ICollection<Turno> Turnos { get; set; } = [];
}
