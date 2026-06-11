using System.ComponentModel.DataAnnotations;

namespace Orders.API.DTOs;

public class SolicitudCrearOrdenItem
{
    [Required(ErrorMessage = "El productoId es obligatorio.")]
    public string ProductoId { get; set; } = string.Empty;

    public int Cantidad { get; set; }
}
