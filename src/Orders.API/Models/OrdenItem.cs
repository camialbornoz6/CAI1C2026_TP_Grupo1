namespace Orders.API.Models;

public class OrdenItem
{
    public string OrdenId { get; set; } = string.Empty;

    public string ProductoId { get; set; } = string.Empty;

    public int Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }
}
