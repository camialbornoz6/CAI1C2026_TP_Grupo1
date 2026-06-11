using Cart.API.DTOs;

namespace Cart.API.Services;

public interface ICarritoServicio
{
    Task<RespuestaCarrito> ObtenerPorUsuarioId(string usuarioId, CancellationToken cancellationToken = default);

    Task<RespuestaCarrito> AgregarItem(string usuarioId, SolicitudAgregarItemCarrito solicitud, CancellationToken cancellationToken = default);

    Task<RespuestaCarrito> ActualizarItem(string usuarioId, string productoId, SolicitudActualizarItemCarrito solicitud, CancellationToken cancellationToken = default);

    Task EliminarItem(string usuarioId, string productoId, CancellationToken cancellationToken = default);

    Task EliminarCarrito(string usuarioId, CancellationToken cancellationToken = default);
}
