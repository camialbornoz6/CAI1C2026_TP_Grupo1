using Microsoft.AspNetCore.Diagnostics;

namespace Notifications.API.ExceptionHandlers;

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
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        _logger.LogError(exception, "Error inesperado al procesar notificacion.");

        var respuesta = ExceptionHandlerHelper.CrearRespuesta(
            context,
            StatusCodes.Status500InternalServerError,
            "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            "Internal Server Error",
            "Error inesperado en el servicio.",
            "NTF-004",
            "Error interno al procesar la notificacion.");

        await context.Response.WriteAsJsonAsync(respuesta, cancellationToken);

        return true;
    }
}
