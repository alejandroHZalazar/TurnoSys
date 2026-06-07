using MediatR;
using Microsoft.Extensions.Logging;

namespace TurnoSys.Application.Common.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var name = typeof(TRequest).Name;
        logger.LogInformation("Ejecutando request: {Name} {@Request}", name, request);

        var response = await next();

        logger.LogInformation("Request completado: {Name}", name);
        return response;
    }
}
