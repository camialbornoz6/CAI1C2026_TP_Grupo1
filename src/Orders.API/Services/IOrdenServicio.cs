using Orders.API.DTOs;

namespace Orders.API.Services;

public interface IOrdenServicio
{
    Task<IEnumerable<RespuestaOrden>> ObtenerTodos(string? usuarioId, CancellationToken cancellationToken = default);

    Task<RespuestaOrden> ObtenerPorId(string id, CancellationToken cancellationToken = default);

    Task<RespuestaOrden> Crear(SolicitudCrearOrden solicitud, CancellationToken cancellationToken = default);

    Task<RespuestaActualizarEstadoOrden> ActualizarEstado(string id, SolicitudActualizarEstadoOrden solicitud, CancellationToken cancellationToken = default);
}
