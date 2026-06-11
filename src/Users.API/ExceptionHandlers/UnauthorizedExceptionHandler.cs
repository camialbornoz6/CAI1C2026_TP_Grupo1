using Microsoft.AspNetCore.Diagnostics;
using Users.API.Exceptions;

namespace Users.API.ExceptionHandlers;

public class UnauthorizedExceptionHandler : IExceptionHandler
{
    private readonly ILogger<UnauthorizedExceptionHandler> _logger;

    public UnauthorizedExceptionHandler(ILogger<UnauthorizedExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not UnauthorizedException ex)
        {
            return false;
        }

        context.Response.StatusCode = StatusCodes.Status401Unauthorized;

        _logger.LogWarning("Autenticacion rechazada. ErrorCode: {ErrorCode}. ErrorMessage: {ErrorMessage}",
            ex.ErrorCode,
            ex.Message);

        var respuesta = ExceptionHandlerHelper.CrearRespuesta(
            context,
            StatusCodes.Status401Unauthorized,
            "https://tools.ietf.org/html/rfc7235#section-3.1",
            "Unauthorized",
            "Las credenciales no son validas.",
            ex.ErrorCode,
            ex.Message);

        await context.Response.WriteAsJsonAsync(respuesta, cancellationToken);

        return true;
    }
}
