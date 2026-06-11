using Users.API.DTOs;

namespace Users.API.ExceptionHandlers;

public static class ExceptionHandlerHelper
{
    public static RespuestaError CrearRespuesta(
        HttpContext context,
        int statusCode,
        string type,
        string title,
        string detail,
        string errorCode,
        string errorMessage)
    {
        string correlationId = context.Response.Headers.TryGetValue("X-Correlation-Id", out var valor)
            ? valor.ToString()
            : context.TraceIdentifier;

        return new RespuestaError
        {
            Type = type,
            Title = title,
            Status = statusCode,
            Detail = detail,
            Instance = context.Request.Path.Value ?? string.Empty,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage,
            CorrelationId = correlationId
        };
    }
}
