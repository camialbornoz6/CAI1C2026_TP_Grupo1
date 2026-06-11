using Microsoft.AspNetCore.Mvc;
using Orders.API.DTOs;
using Orders.API.Services;

namespace Orders.API.Controllers;

[ApiController]
[Route("api/orders")]
[Produces("application/json")]
public class OrdenesController : ControllerBase
{
    private readonly IOrdenServicio _ordenServicio;

    public OrdenesController(IOrdenServicio ordenServicio)
    {
        _ordenServicio = ordenServicio;
    }

    /// <summary>
    /// Lista ordenes. Permite filtrar por usuarioId.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RespuestaOrden>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<RespuestaOrden>>> ObtenerTodos(
        [FromQuery] string? usuarioId,
        CancellationToken cancellationToken)
    {
        IEnumerable<RespuestaOrden> ordenes = await _ordenServicio.ObtenerTodos(usuarioId, cancellationToken);

        return Ok(ordenes);
    }

    /// <summary>
    /// Obtiene una orden por su identificador.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(RespuestaOrden), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RespuestaOrden>> ObtenerPorId(string id, CancellationToken cancellationToken)
    {
        RespuestaOrden orden = await _ordenServicio.ObtenerPorId(id, cancellationToken);

        return Ok(orden);
    }

    /// <summary>
    /// Crea una nueva orden validando usuario, productos y stock mediante comunicacion HTTP con otros servicios.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(RespuestaOrden), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RespuestaOrden>> Crear(
        SolicitudCrearOrden solicitud,
        CancellationToken cancellationToken)
    {
        RespuestaOrden ordenCreada = await _ordenServicio.Crear(solicitud, cancellationToken);

        return CreatedAtAction(
            nameof(ObtenerPorId),
            new { id = ordenCreada.Id },
            ordenCreada);
    }

    /// <summary>
    /// Actualiza el estado de una orden existente.
    /// </summary>
    [HttpPut("{id}/status")]
    [ProducesResponseType(typeof(RespuestaActualizarEstadoOrden), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RespuestaActualizarEstadoOrden>> ActualizarEstado(
        string id,
        SolicitudActualizarEstadoOrden solicitud,
        CancellationToken cancellationToken)
    {
        RespuestaActualizarEstadoOrden respuesta = await _ordenServicio.ActualizarEstado(id, solicitud, cancellationToken);

        return Ok(respuesta);
    }
}
