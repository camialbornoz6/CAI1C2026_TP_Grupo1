using Notifications.API.DTOs;

namespace Notifications.API.Services;

public interface INotificacionServicio
{
    Task<RespuestaNotificacion> Enviar(SolicitudEnviarNotificacion solicitud, CancellationToken cancellationToken = default);

    Task<IEnumerable<RespuestaNotificacion>> ObtenerPorUsuarioId(string usuarioId, CancellationToken cancellationToken = default);
}
