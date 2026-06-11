using Cart.API.Models;

namespace Cart.API.Repositories;

public interface ICarritoRepositorio
{
    Task<Carrito?> ObtenerPorUsuarioId(string usuarioId, CancellationToken cancellationToken = default);

    Task GuardarItem(string usuarioId, string productoId, int cantidad, DateTime fechaActualizacion, CancellationToken cancellationToken = default);

    Task ActualizarItem(string usuarioId, string productoId, int cantidad, DateTime fechaActualizacion, CancellationToken cancellationToken = default);

    Task EliminarItem(string usuarioId, string productoId, DateTime fechaActualizacion, CancellationToken cancellationToken = default);

    Task EliminarCarrito(string usuarioId, CancellationToken cancellationToken = default);
}
