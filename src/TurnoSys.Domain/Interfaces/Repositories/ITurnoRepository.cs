using TurnoSys.Domain.Entities;

namespace TurnoSys.Domain.Interfaces.Repositories;

public interface ITurnoRepository : IGenericRepository<Turno>
{
    Task<IEnumerable<Turno>> GetByProfesionalAsync(
        Guid profesionalId, DateTime desde, DateTime hasta, CancellationToken ct = default);

    Task<IEnumerable<Turno>> GetByEmpresaAsync(
        Guid empresaId, DateTime desde, DateTime hasta, CancellationToken ct = default);

    Task<bool> ExisteSolapamientoAsync(
        Guid profesionalId, DateTime inicio, DateTime fin,
        Guid? turnoExcluirId = null, CancellationToken ct = default);

    Task<IEnumerable<Turno>> GetPendientesRecordatorioAsync(
        DateTime fechaObjetivo, CancellationToken ct = default);
}
