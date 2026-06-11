namespace Notifications.API.Clients;

public class UsuarioExterno
{
    public string Id { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public DateTime FechaRegistro { get; set; }

    public bool Activo { get; set; }
}
