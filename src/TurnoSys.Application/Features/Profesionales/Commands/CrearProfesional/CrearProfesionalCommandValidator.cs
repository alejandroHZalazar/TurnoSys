using FluentValidation;

namespace TurnoSys.Application.Features.Profesionales.Commands.CrearProfesional;

public class CrearProfesionalCommandValidator : AbstractValidator<CrearProfesionalCommand>
{
    public CrearProfesionalCommandValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Apellido).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).EmailAddress().MaximumLength(200).When(x => !string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.Telefono).MaximumLength(50).When(x => x.Telefono != null);
        RuleFor(x => x.ColorAgenda).NotEmpty().Matches(@"^#[0-9A-Fa-f]{6}$").WithMessage("Color debe ser un HEX válido (#RRGGBB)");
        RuleForEach(x => x.Horarios).ChildRules(h => {
            h.RuleFor(x => x.DiaSemana).InclusiveBetween(0, 6);
            h.RuleFor(x => x.HoraFin).GreaterThan(x => x.HoraInicio).WithMessage("HoraFin debe ser mayor a HoraInicio");
        });
    }
}
