using MediatR;

namespace TurnoSys.Application.Features.Practicas.Queries.GetPracticas;

public record GetPracticasQuery(
    Guid EmpresaId,
    bool? SoloActivas = true
) : IRequest<IEnumerable<PracticaListDto>>;

public record PracticaListDto(
    Guid Id,
    string Nombre,
    string? Descripcion,
    decimal Precio,
    int DuracionMinutos,
    string? Color,
    bool RequiereObservaciones,
    int? RecordatorioRecDias,
    bool IsActivo,
    Guid? CategoriaId,
    string? CategoriaNombre,
    string? CategoriaColor
);
