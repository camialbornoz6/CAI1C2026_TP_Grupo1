namespace Notifications.API.Models;

public class Notificacion
{
    public string Id { get; set; } = string.Empty;

    public string UsuarioId { get; set; } = string.Empty;

    public string Mensaje { get; set; } = string.Empty;

    public string Tipo { get; set; } = string.Empty;

    public string Estado { get; set; } = string.Empty;

    public DateTime FechaEnvio { get; set; }
}
