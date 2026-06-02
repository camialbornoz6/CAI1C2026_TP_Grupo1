using Microsoft.AspNetCore.Diagnostics;
using Products.API.Exceptions;

namespace Products.API.ExceptionHandlers;

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

        _logger.LogWarning("Regla de negocio incumplida. ErrorCode: {ErrorCode}. Path: {Path}",
            ex.ErrorCode,
            context.Request.Path.Value);

        context.Response.StatusCode = StatusCodes.Status409Conflict;

        string detail = ex.ErrorCode == "PRD-004"
            ? "No se puede eliminar el recurso."
            : "Ya existe un recurso con esos datos.";

        var respuesta = ExceptionHandlerHelper.CrearRespuestaError(
            context,
            StatusCodes.Status409Conflict,
            "https://tools.ietf.org/html/rfc7231#section-6.5.9",
            "Conflict",
            detail,
            ex.ErrorCode,
            ex.Message);

        await context.Response.WriteAsJsonAsync(respuesta, cancellationToken);

        return true;
    }
}
