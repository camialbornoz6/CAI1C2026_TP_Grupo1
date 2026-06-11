using Notifications.API.Models;

namespace Notifications.API.Repositories;

public interface INotificacionRepositorio
{
    Task<IEnumerable<Notificacion>> ObtenerPorUsuarioId(string usuarioId, CancellationToken cancellationToken = default);

    Task Crear(Notificacion notificacion, CancellationToken cancellationToken = default);
}
