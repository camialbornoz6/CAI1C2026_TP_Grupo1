using Microsoft.AspNetCore.Mvc;
using Users.API.DTOs;
using Users.API.Services;

namespace Users.API.Controllers;

[ApiController]
[Route("api/users")]
[Produces("application/json")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioServicio _usuarioServicio;

    public UsuariosController(IUsuarioServicio usuarioServicio)
    {
        _usuarioServicio = usuarioServicio;
    }


    /// <summary>
    /// Obtiene datos publicos de un usuario por ID. Endpoint tecnico para validacion entre microservicios.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(RespuestaUsuario), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RespuestaUsuario>> ObtenerPorId(
        string id,
        CancellationToken cancellationToken)
    {
        RespuestaUsuario? usuario = await _usuarioServicio.ObtenerPorId(id, cancellationToken);

        if (usuario == null)
        {
            return NotFound();
        }

        return Ok(usuario);
    }

    /// <summary>
    /// Registra un nuevo usuario. PasswordHash no se devuelve en la respuesta.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(RespuestaUsuario), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RespuestaUsuario>> Registrar(
        SolicitudRegistrarUsuario solicitud,
        CancellationToken cancellationToken)
    {
        RespuestaUsuario usuarioRegistrado = await _usuarioServicio.Registrar(solicitud, cancellationToken);

        return Created("/api/users/register", usuarioRegistrado);
    }

    /// <summary>
    /// Autentica un usuario por email y password. Al tercer intento fallido consecutivo bloquea el usuario.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(RespuestaLogin), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RespuestaLogin>> Login(
        SolicitudLogin solicitud,
        CancellationToken cancellationToken)
    {
        RespuestaLogin respuesta = await _usuarioServicio.Login(solicitud, cancellationToken);

        return Ok(respuesta);
    }
}
