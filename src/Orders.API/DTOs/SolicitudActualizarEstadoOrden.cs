using System.ComponentModel.DataAnnotations;

namespace Orders.API.DTOs;

public class SolicitudActualizarEstadoOrden
{
    [Required(ErrorMessage = "El estado es obligatorio.")]
    public string Estado { get; set; } = string.Empty;
}
