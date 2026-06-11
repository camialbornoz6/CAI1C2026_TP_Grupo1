using System.ComponentModel.DataAnnotations;

namespace Users.API.DTOs;

public class SolicitudLogin
{
    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "El email debe tener un formato valido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La password es obligatoria.")]
    public string Password { get; set; } = string.Empty;
}
