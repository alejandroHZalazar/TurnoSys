using FluentValidation;

namespace TurnoSys.Application.Features.Practicas.Commands.CrearPractica;

public class CrearPracticaCommandValidator : AbstractValidator<CrearPracticaCommand>
{
    public CrearPracticaCommandValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Precio).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DuracionMinutos).GreaterThan(0).LessThanOrEqualTo(480);
        RuleFor(x => x.Color).Matches(@"^#[0-9A-Fa-f]{6}$").When(x => !string.IsNullOrEmpty(x.Color));
        RuleFor(x => x.RecordatorioRecDias).GreaterThan(0).When(x => x.RecordatorioRecDias.HasValue);
    }
}
