using System.ComponentModel.DataAnnotations;

namespace Notifications.API.DTOs;

public class SolicitudEnviarNotificacion
{
    [Required(ErrorMessage = "El usuarioId es obligatorio.")]
    public string UsuarioId { get; set; } = string.Empty;

    [Required(ErrorMessage = "El mensaje es obligatorio.")]
    [MaxLength(500, ErrorMessage = "El mensaje no puede superar los 500 caracteres.")]
    public string Mensaje { get; set; } = string.Empty;

    [Required(ErrorMessage = "El tipo de notificacion es obligatorio.")]
    public string Tipo { get; set; } = string.Empty;
}
