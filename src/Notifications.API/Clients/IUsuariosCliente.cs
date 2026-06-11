namespace Notifications.API.Clients;

public interface IUsuariosCliente
{
    Task<bool> ExisteUsuario(string usuarioId, CancellationToken cancellationToken = default);
}
