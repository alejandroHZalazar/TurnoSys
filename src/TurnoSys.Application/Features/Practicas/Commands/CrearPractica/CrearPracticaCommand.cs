using MediatR;
using TurnoSys.Application.Common.Models;

namespace TurnoSys.Application.Features.Practicas.Commands.CrearPractica;

public record CrearPracticaCommand(
    Guid EmpresaId,
    Guid? CategoriaId,
    string Nombre,
    string? Descripcion,
    decimal Precio,
    int DuracionMinutos,
    string? Color,
    bool RequiereObservaciones,
    int? RecordatorioRecDias
) : IRequest<Result<Guid>>;
