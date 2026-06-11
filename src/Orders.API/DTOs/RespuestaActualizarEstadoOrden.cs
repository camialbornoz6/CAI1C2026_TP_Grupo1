namespace Orders.API.DTOs;

public class RespuestaActualizarEstadoOrden
{
    public string Id { get; set; } = string.Empty;

    public string Estado { get; set; } = string.Empty;

    public DateTime FechaActualizacion { get; set; }
}
