using Cart.API.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace Cart.API.ExceptionHandlers;

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

        context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;

        _logger.LogWarning("Regla de negocio de carrito incumplida. ErrorCode: {ErrorCode}. Mensaje: {Mensaje}", ex.ErrorCode, ex.Message);

        var respuesta = ExceptionHandlerHelper.CrearRespuesta(
            context,
            StatusCodes.Status422UnprocessableEntity,
            "https://tools.ietf.org/html/rfc4918#section-11.2",
            "Unprocessable Entity",
            "No se puede procesar la solicitud.",
            ex.ErrorCode,
            ex.Message);

        await context.Response.WriteAsJsonAsync(respuesta, cancellationToken);

        return true;
    }
}
