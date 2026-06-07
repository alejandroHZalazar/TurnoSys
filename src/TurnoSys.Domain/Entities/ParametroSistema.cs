using TurnoSys.Domain.Entities.Common;

namespace TurnoSys.Domain.Entities;

public class ParametroSistema : BaseEntity
{
    public Guid? EmpresaId { get; set; }
    public string Clave { get; set; } = string.Empty;
    public string? Valor { get; set; }
    public string TipoDato { get; set; } = "string";
    public string? Descripcion { get; set; }
    public bool IsEncriptado { get; set; } = false;
    public bool IsGlobal { get; set; } = false;
    public Guid? UpdatedBy { get; set; }

    public Empresa? Empresa { get; set; }
}
