using Microsoft.AspNetCore.Diagnostics;
using Orders.API.Exceptions;

namespace Orders.API.ExceptionHandlers;

public class BusinessRuleExceptionHandler : IExceptionHandler
{
    private readonly ILogger<BusinessRuleExceptionHandler> _logger;

    public BusinessRuleExceptionHandler(ILogger<BusinessRuleExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not BusinessRuleException ex)
        {
            return false;
        }

        int statusCode = ObtenerStatusCode(ex.ErrorCode);
        string title = statusCode == StatusCodes.Status422UnprocessableEntity
            ? "Unprocessable Entity"
            : "Conflict";
        string type = statusCode == StatusCodes.Status422UnprocessableEntity
            ? "https://tools.ietf.org/html/rfc4918#section-11.2"
            : "https://tools.ietf.org/html/rfc7231#section-6.5.9";
        string detail = statusCode == StatusCodes.Status422UnprocessableEntity
            ? "No se puede procesar la solicitud."
            : "No se puede modificar el recurso.";

        context.Response.StatusCode = statusCode;

        _logger.LogWarning("Regla de negocio de orden incumplida. ErrorCode: {ErrorCode}. Mensaje: {Mensaje}", ex.ErrorCode, ex.Message);

        var respuesta = ExceptionHandlerHelper.CrearRespuesta(
            context,
            statusCode,
            type,
            title,
            detail,
            ex.ErrorCode,
            ex.Message);

        await context.Response.WriteAsJsonAsync(respuesta, cancellationToken);

        return true;
    }

    private static int ObtenerStatusCode(string errorCode)
    {
        return errorCode switch
        {
            "ORD-005" => StatusCodes.Status422UnprocessableEntity,
            "ORD-006" => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status409Conflict
        };
    }
}
