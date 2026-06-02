using Microsoft.AspNetCore.Mvc;
using Products.API.DTOs;
using Products.API.Services;

namespace Products.API.Controllers;

[ApiController]
[Route("api/products")]
[Produces("application/json")]
public class ProductosController : ControllerBase
{
    private readonly IProductoServicio _productoServicio;

    public ProductosController(IProductoServicio productoServicio)
    {
        _productoServicio = productoServicio;
    }

    /// <summary>
    /// Lista productos. Permite filtrar por categoria y por parte del nombre.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RespuestaProducto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status500InternalServerError)]
    public ActionResult<IEnumerable<RespuestaProducto>> ObtenerTodos(
        [FromQuery] string? categoria,
        [FromQuery] string? nombre)
    {
        IEnumerable<RespuestaProducto> productos = _productoServicio.ObtenerTodos(categoria, nombre);

        return Ok(productos);
    }

    /// <summary>
    /// Obtiene un producto por su identificador.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(RespuestaProducto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status500InternalServerError)]
    public ActionResult<RespuestaProducto> ObtenerPorId(string id)
    {
        RespuestaProducto producto = _productoServicio.ObtenerPorId(id);

        return Ok(producto);
    }

    /// <summary>
    /// Crea un nuevo producto.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(RespuestaProducto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status500InternalServerError)]
    public ActionResult<RespuestaProducto> Crear(SolicitudCrearProducto solicitud)
    {
        RespuestaProducto productoCreado = _productoServicio.Crear(solicitud);

        return CreatedAtAction(
            nameof(ObtenerPorId),
            new { id = productoCreado.Id },
            productoCreado);
    }

    /// <summary>
    /// Actualiza un producto existente.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(RespuestaProducto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status500InternalServerError)]
    public ActionResult<RespuestaProducto> Actualizar(string id, SolicitudActualizarProducto solicitud)
    {
        RespuestaProducto productoActualizado = _productoServicio.Actualizar(id, solicitud);

        return Ok(productoActualizado);
    }

    /// <summary>
    /// Elimina un producto existente.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status500InternalServerError)]
    public IActionResult Eliminar(string id)
    {
        _productoServicio.Eliminar(id);

        return NoContent();
    }
}
