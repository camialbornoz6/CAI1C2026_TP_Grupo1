namespace Users.API.Models;

public class Usuario
{
    public string Id { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public DateTime FechaRegistro { get; set; }

    public bool Activo { get; set; }

    public int IntentosFallidos { get; set; }

    public string? MotivoBloqueo { get; set; }
}
