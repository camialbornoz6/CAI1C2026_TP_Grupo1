using Products.API.DTOs;

namespace Products.API.ExceptionHandlers;

public static class ExceptionHandlerHelper
{
    public static RespuestaError CrearRespuestaError(
        HttpContext context,
        int status,
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
            Status = status,
            Detail = detail,
            Instance = context.Request.Path.Value ?? string.Empty,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage,
            CorrelationId = correlationId
        };
    }
}
