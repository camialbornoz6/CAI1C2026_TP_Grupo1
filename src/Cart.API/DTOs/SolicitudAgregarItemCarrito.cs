using System.ComponentModel.DataAnnotations;

namespace Cart.API.DTOs;

public class SolicitudAgregarItemCarrito
{
    [Required(ErrorMessage = "El productoId es obligatorio.")]
    public string ProductoId { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero.")]
    public int Cantidad { get; set; }
}
