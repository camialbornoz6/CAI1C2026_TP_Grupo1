using Microsoft.AspNetCore.Diagnostics;

namespace Products.API.ExceptionHandlers;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Error inesperado al procesar Products.API. ErrorCode: {ErrorCode}. Path: {Path}",
            "PRD-005",
            context.Request.Path.Value);

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var respuesta = ExceptionHandlerHelper.CrearRespuestaError(
            context,
            StatusCodes.Status500InternalServerError,
            "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            "Internal Server Error",
            "Error interno al procesar el producto.",
            "PRD-005",
            "Error interno al procesar el producto.");

        await context.Response.WriteAsJsonAsync(respuesta, cancellationToken);

        return true;
    }
}
