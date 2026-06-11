namespace Orders.API.DTOs;

public class RespuestaOrdenItem
{
    public string ProductoId { get; set; } = string.Empty;

    public int Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }
}
