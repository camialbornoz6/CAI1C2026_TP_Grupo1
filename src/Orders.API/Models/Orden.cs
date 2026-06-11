namespace Orders.API.Models;

public class Orden
{
    public string Id { get; set; } = string.Empty;

    public string UsuarioId { get; set; } = string.Empty;

    public decimal Total { get; set; }

    public string Estado { get; set; } = string.Empty;

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public List<OrdenItem> Items { get; set; } = new();
}
