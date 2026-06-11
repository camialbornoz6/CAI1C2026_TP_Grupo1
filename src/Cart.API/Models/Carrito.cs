namespace Cart.API.Models;

public class Carrito
{
    public string UsuarioId { get; set; } = string.Empty;

    public List<CarritoItem> Items { get; set; } = new();

    public DateTime FechaActualizacion { get; set; }
}
