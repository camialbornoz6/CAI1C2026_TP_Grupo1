using Microsoft.AspNetCore.Diagnostics;
using ValidationException = Cart.API.Exceptions.ValidationException;

namespace Cart.API.ExceptionHandlers;

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

        context.Response.StatusCode = StatusCodes.Status400BadRequest;

        _logger.LogWarning("Validacion de carrito fallida. ErrorCode: {ErrorCode}. Mensaje: {Mensaje}", ex.ErrorCode, ex.Message);

        var respuesta = ExceptionHandlerHelper.CrearRespuesta(
            context,
            StatusCodes.Status400BadRequest,
            "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            "Bad Request",
            "Los datos del carrito son invalidos.",
            ex.ErrorCode,
            ex.Message);

        await context.Response.WriteAsJsonAsync(respuesta, cancellationToken);

        return true;
    }
}
