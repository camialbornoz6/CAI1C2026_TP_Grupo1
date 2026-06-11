using System.ComponentModel.DataAnnotations;

namespace Orders.API.DTOs;

public class SolicitudCrearOrden
{
    [Required(ErrorMessage = "El usuarioId es obligatorio.")]
    public string UsuarioId { get; set; } = string.Empty;

    [Required(ErrorMessage = "La orden debe tener al menos un item.")]
    public List<SolicitudCrearOrdenItem> Items { get; set; } = new();
}
