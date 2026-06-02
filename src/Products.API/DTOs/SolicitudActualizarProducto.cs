using System.ComponentModel.DataAnnotations;

namespace Products.API.DTOs;

/// <summary>
/// Datos necesarios para actualizar un producto existente.
/// </summary>
public class SolicitudActualizarProducto
{
    [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "La descripcion no puede superar los 500 caracteres.")]
    public string? Descripcion { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a cero.")]
    public decimal Precio { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
    public int Stock { get; set; }

    [Required(ErrorMessage = "La categoria es obligatoria.")]
    public string Categoria { get; set; } = string.Empty;
}
