using MediatR;

namespace TurnoSys.Application.Features.Empresa.Queries.GetLogo;

public record GetLogoQuery(Guid EmpresaId) : IRequest<LogoResult?>;

public record LogoResult(byte[] Contenido, string ContentType);
