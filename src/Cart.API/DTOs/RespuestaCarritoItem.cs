namespace Cart.API.DTOs;

public class RespuestaCarritoItem
{
    public string ProductoId { get; set; } = string.Empty;

    public int Cantidad { get; set; }
}
