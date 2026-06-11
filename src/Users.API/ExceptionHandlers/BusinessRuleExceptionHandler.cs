using Microsoft.AspNetCore.Diagnostics;
using Users.API.Exceptions;

namespace Users.API.ExceptionHandlers;

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

        context.Response.StatusCode = StatusCodes.Status409Conflict;

        _logger.LogWarning("Regla de negocio de usuario rechazada. ErrorCode: {ErrorCode}. ErrorMessage: {ErrorMessage}",
            ex.ErrorCode,
            ex.Message);

        var respuesta = ExceptionHandlerHelper.CrearRespuesta(
            context,
            StatusCodes.Status409Conflict,
            "https://tools.ietf.org/html/rfc7231#section-6.5.9",
            "Conflict",
            "Ya existe un recurso con esos datos.",
            ex.ErrorCode,
            ex.Message);

        await context.Response.WriteAsJsonAsync(respuesta, cancellationToken);

        return true;
    }
}
