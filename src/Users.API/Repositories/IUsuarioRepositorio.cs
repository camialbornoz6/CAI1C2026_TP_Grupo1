using Users.API.Models;

namespace Users.API.Repositories;

public interface IUsuarioRepositorio
{
    Task<Usuario?> ObtenerPorId(string id, CancellationToken cancellationToken = default);

    Task<Usuario?> ObtenerPorEmail(string email, CancellationToken cancellationToken = default);

    Task Crear(Usuario usuario, CancellationToken cancellationToken = default);

    Task<int> IncrementarIntentoFallido(string email, CancellationToken cancellationToken = default);

    Task BloquearPorIntentosFallidos(string email, CancellationToken cancellationToken = default);

    Task ReiniciarIntentosFallidos(string email, CancellationToken cancellationToken = default);
}
