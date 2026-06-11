using Orders.API.Models;

namespace Orders.API.Repositories;

public interface IOrdenRepositorio
{
    Task<IEnumerable<Orden>> ObtenerTodos(string? usuarioId, CancellationToken cancellationToken = default);

    Task<Orden?> ObtenerPorId(string id, CancellationToken cancellationToken = default);

    Task Crear(Orden orden, CancellationToken cancellationToken = default);

    Task ActualizarEstado(string id, string estado, DateTime fechaActualizacion, CancellationToken cancellationToken = default);
}
