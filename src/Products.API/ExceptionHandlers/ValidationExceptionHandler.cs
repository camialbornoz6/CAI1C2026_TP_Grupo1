using Microsoft.AspNetCore.Diagnostics;
using Products.API.Exceptions;

namespace Products.API.ExceptionHandlers;

public class ValidationExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ValidationExceptionHandler> _logger;

    public ValidationExceptionHandler(ILogger<ValidationExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ValidationException ex)
        {
            return false;
        }

        _logger.LogWarning("Datos invalidos. ErrorCode: {ErrorCode}. Path: {Path}",
            ex.ErrorCode,
            context.Request.Path.Value);

        context.Response.StatusCode = StatusCodes.Status400BadRequest;

        var respuesta = ExceptionHandlerHelper.CrearRespuestaError(
            context,
            StatusCodes.Status400BadRequest,
            "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            "Bad Request",
            "Los datos del producto son invalidos.",
            ex.ErrorCode,
            ex.Message);

        await context.Response.WriteAsJsonAsync(respuesta, cancellationToken);

        return true;
    }
}
