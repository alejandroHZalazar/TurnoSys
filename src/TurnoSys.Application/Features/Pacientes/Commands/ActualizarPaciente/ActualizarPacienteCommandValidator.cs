using FluentValidation;

namespace TurnoSys.Application.Features.Pacientes.Commands.ActualizarPaciente;

public class ActualizarPacienteCommandValidator : AbstractValidator<ActualizarPacienteCommand>
{
    public ActualizarPacienteCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Apellido).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DNI).MaximumLength(20).When(x => x.DNI != null);
        RuleFor(x => x.Email).EmailAddress().MaximumLength(200).When(x => !string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.Telefono).MaximumLength(50).When(x => x.Telefono != null);
    }
}
