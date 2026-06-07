using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using TurnoSys.Application.Common.Exceptions;
using TurnoSys.Domain.Exceptions;
using ValidationException = TurnoSys.Application.Common.Exceptions.ValidationException;

namespace TurnoSys.API.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Excepción no controlada: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, problem) = exception switch
        {
            ValidationException ve => (
                HttpStatusCode.UnprocessableEntity,
                new ValidationProblemDetails(ve.Errors)
                {
                    Status = 422,
                    Title = "Error de validación",
                    Type = "https://turnosys.com/errors/validation"
                }),

            NotFoundException nfe => (
                HttpStatusCode.NotFound,
                new ProblemDetails
                {
                    Status = 404,
                    Title = "Recurso no encontrado",
                    Detail = nfe.Message,
                    Type = "https://turnosys.com/errors/not-found"
                }),

            ForbiddenException fe => (
                HttpStatusCode.Forbidden,
                new ProblemDetails
                {
                    Status = 403,
                    Title = "Acceso denegado",
                    Detail = fe.Message
                }),

            DomainException de => (
                HttpStatusCode.Conflict,
                new ProblemDetails
                {
                    Status = 409,
                    Title = "Regla de negocio violada",
                    Detail = de.Message,
                    Type = "https://turnosys.com/errors/conflict"
                }),

            _ => (
                HttpStatusCode.InternalServerError,
                new ProblemDetails
                {
                    Status = 500,
                    Title = "Error interno del servidor",
                    Type = "https://turnosys.com/errors/internal"
                })
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        problem.Extensions["traceId"] = context.TraceIdentifier;
        // Serializar con el tipo runtime para no perder 'errors' de ValidationProblemDetails
        await context.Response.WriteAsync(JsonSerializer.Serialize<object>(problem));
    }
}
