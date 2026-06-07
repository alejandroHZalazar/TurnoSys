namespace TurnoSys.Application.Common.Exceptions;

public class ForbiddenException : Exception
{
    public ForbiddenException() : base("No tiene permisos para realizar esta acción.") { }
    public ForbiddenException(string message) : base(message) { }
}
