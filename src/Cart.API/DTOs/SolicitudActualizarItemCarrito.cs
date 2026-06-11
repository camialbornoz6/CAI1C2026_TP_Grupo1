using System.ComponentModel.DataAnnotations;

namespace Cart.API.DTOs;

public class SolicitudActualizarItemCarrito
{
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero.")]
    public int Cantidad { get; set; }
}
