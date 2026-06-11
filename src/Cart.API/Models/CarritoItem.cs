namespace Cart.API.Models;

public class CarritoItem
{
    public string UsuarioId { get; set; } = string.Empty;

    public string ProductoId { get; set; } = string.Empty;

    public int Cantidad { get; set; }
}
