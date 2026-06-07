using MediatR;
using TurnoSys.Application.Common.Models;

namespace TurnoSys.Application.Features.Practicas.Commands.ActualizarPractica;

public record ActualizarPracticaCommand(
    Guid Id,
    Guid EmpresaId,
    Guid? CategoriaId,
    string Nombre,
    string? Descripcion,
    decimal Precio,
    int DuracionMinutos,
    string? Color,
    bool RequiereObservaciones,
    int? RecordatorioRecDias,
    bool IsActivo
) : IRequest<Result>;
