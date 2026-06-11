using Microsoft.AspNetCore.Mvc;
using Notifications.API.DTOs;
using Notifications.API.Services;

namespace Notifications.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Produces("application/json")]
public class NotificacionesController : ControllerBase
{
    private readonly INotificacionServicio _notificacionServicio;

    public NotificacionesController(INotificacionServicio notificacionServicio)
    {
        _notificacionServicio = notificacionServicio;
    }

    /// <summary>
    /// Registra y simula el envio de una notificacion al usuario destinatario.
    /// </summary>
    [HttpPost("send")]
    [ProducesResponseType(typeof(RespuestaNotificacion), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RespuestaNotificacion>> Enviar(
        SolicitudEnviarNotificacion solicitud,
        CancellationToken cancellationToken)
    {
        RespuestaNotificacion respuesta = await _notificacionServicio.Enviar(solicitud, cancellationToken);

        return Created($"/api/notifications/{respuesta.UsuarioId}", respuesta);
    }

    /// <summary>
    /// Lista las notificaciones registradas para un usuario.
    /// </summary>
    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(IEnumerable<RespuestaNotificacion>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<RespuestaNotificacion>>> ObtenerPorUsuarioId(
        string userId,
        CancellationToken cancellationToken)
    {
        IEnumerable<RespuestaNotificacion> respuesta = await _notificacionServicio.ObtenerPorUsuarioId(userId, cancellationToken);

        return Ok(respuesta);
    }
}
