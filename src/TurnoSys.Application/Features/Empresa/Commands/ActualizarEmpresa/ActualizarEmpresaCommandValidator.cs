using FluentValidation;

namespace TurnoSys.Application.Features.Empresa.Commands.ActualizarEmpresa;

public class ActualizarEmpresaCommandValidator : AbstractValidator<ActualizarEmpresaCommand>
{
    public ActualizarEmpresaCommandValidator()
    {
        RuleFor(x => x.RazonSocial).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NombreFantasia).MaximumLength(200).When(x => x.NombreFantasia != null);
        RuleFor(x => x.CUIT).MaximumLength(20).When(x => x.CUIT != null);
        RuleFor(x => x.Email).EmailAddress().MaximumLength(200).When(x => !string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.SitioWeb).MaximumLength(300).When(x => x.SitioWeb != null);
    }
}
