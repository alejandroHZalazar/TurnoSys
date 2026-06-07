using MediatR;
using TurnoSys.Application.Features.Practicas.Queries.GetPracticas;

namespace TurnoSys.Application.Features.Practicas.Queries.GetPracticaById;

public record GetPracticaByIdQuery(Guid Id, Guid EmpresaId) : IRequest<PracticaListDto?>;
