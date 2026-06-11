namespace Cart.API.DTOs;

public class RespuestaCarrito
{
    public string UsuarioId { get; set; } = string.Empty;

    public List<RespuestaCarritoItem> Items { get; set; } = new();

    public DateTime FechaActualizacion { get; set; }
}
