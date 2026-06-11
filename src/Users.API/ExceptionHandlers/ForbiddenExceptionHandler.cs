using Microsoft.AspNetCore.Diagnostics;
using Users.API.Exceptions;

namespace Users.API.ExceptionHandlers;

public class ForbiddenExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ForbiddenExceptionHandler> _logger;

    public ForbiddenExceptionHandler(ILogger<ForbiddenExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ForbiddenException ex)
        {
            return false;
        }

        context.Response.StatusCode = StatusCodes.Status403Forbidden;

        _logger.LogWarning("Acceso de usuario prohibido. ErrorCode: {ErrorCode}. ErrorMessage: {ErrorMessage}",
            ex.ErrorCode,
            ex.Message);

        var respuesta = ExceptionHandlerHelper.CrearRespuesta(
            context,
            StatusCodes.Status403Forbidden,
            "https://tools.ietf.org/html/rfc7231#section-6.5.3",
            "Forbidden",
            "El acceso esta prohibido.",
            ex.ErrorCode,
            ex.Message);

        await context.Response.WriteAsJsonAsync(respuesta, cancellationToken);

        return true;
    }
}
