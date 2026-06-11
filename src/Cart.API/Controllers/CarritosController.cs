using Cart.API.DTOs;
using Cart.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Cart.API.Controllers;

[ApiController]
[Route("api/cart")]
[Produces("application/json")]
public class CarritosController : ControllerBase
{
    private readonly ICarritoServicio _carritoServicio;

    public CarritosController(ICarritoServicio carritoServicio)
    {
        _carritoServicio = carritoServicio;
    }

    /// <summary>
    /// Obtiene el carrito activo del usuario.
    /// </summary>
    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(RespuestaCarrito), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RespuestaCarrito>> ObtenerPorUsuarioId(
        string userId,
        CancellationToken cancellationToken)
    {
        RespuestaCarrito carrito = await _carritoServicio.ObtenerPorUsuarioId(userId, cancellationToken);

        return Ok(carrito);
    }

    /// <summary>
    /// Agrega un producto al carrito. Si el producto ya existe, suma la cantidad solicitada.
    /// </summary>
    [HttpPost("{userId}/items")]
    [ProducesResponseType(typeof(RespuestaCarrito), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RespuestaCarrito>> AgregarItem(
        string userId,
        SolicitudAgregarItemCarrito solicitud,
        CancellationToken cancellationToken)
    {
        RespuestaCarrito carrito = await _carritoServicio.AgregarItem(userId, solicitud, cancellationToken);

        return Ok(carrito);
    }

    /// <summary>
    /// Actualiza la cantidad de un item existente del carrito.
    /// </summary>
    [HttpPut("{userId}/items/{productId}")]
    [ProducesResponseType(typeof(RespuestaCarrito), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RespuestaCarrito>> ActualizarItem(
        string userId,
        string productId,
        SolicitudActualizarItemCarrito solicitud,
        CancellationToken cancellationToken)
    {
        RespuestaCarrito carrito = await _carritoServicio.ActualizarItem(userId, productId, solicitud, cancellationToken);

        return Ok(carrito);
    }

    /// <summary>
    /// Quita un producto del carrito.
    /// </summary>
    [HttpDelete("{userId}/items/{productId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> EliminarItem(
        string userId,
        string productId,
        CancellationToken cancellationToken)
    {
        await _carritoServicio.EliminarItem(userId, productId, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Vacia el carrito completo del usuario.
    /// </summary>
    [HttpDelete("{userId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> EliminarCarrito(
        string userId,
        CancellationToken cancellationToken)
    {
        await _carritoServicio.EliminarCarrito(userId, cancellationToken);

        return NoContent();
    }
}
