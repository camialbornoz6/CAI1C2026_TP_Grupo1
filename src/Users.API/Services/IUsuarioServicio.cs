using Users.API.DTOs;

namespace Users.API.Services;

public interface IUsuarioServicio
{
    Task<RespuestaUsuario?> ObtenerPorId(string id, CancellationToken cancellationToken = default);

    Task<RespuestaUsuario> Registrar(SolicitudRegistrarUsuario solicitud, CancellationToken cancellationToken = default);

    Task<RespuestaLogin> Login(SolicitudLogin solicitud, CancellationToken cancellationToken = default);
}
