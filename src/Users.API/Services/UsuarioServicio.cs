using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Users.API.DTOs;
using Users.API.Exceptions;
using Users.API.Models;
using Users.API.Repositories;
using ValidationException = Users.API.Exceptions.ValidationException;

namespace Users.API.Services;

public class UsuarioServicio : IUsuarioServicio
{
    private const int MaximoIntentosFallidos = 3;

    private readonly IUsuarioRepositorio _usuarioRepositorio;
    private readonly ILogger<UsuarioServicio> _logger;

    public UsuarioServicio(IUsuarioRepositorio usuarioRepositorio, ILogger<UsuarioServicio> logger)
    {
        _usuarioRepositorio = usuarioRepositorio;
        _logger = logger;
    }


    public async Task<RespuestaUsuario?> ObtenerPorId(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        Usuario? usuario = await _usuarioRepositorio.ObtenerPorId(id.Trim(), cancellationToken);

        return usuario == null ? null : ConvertirARespuestaUsuario(usuario);
    }

    public async Task<RespuestaUsuario> Registrar(
        SolicitudRegistrarUsuario solicitud,
        CancellationToken cancellationToken = default)
    {
        ValidarSolicitudRegistro(solicitud);

        string emailNormalizado = NormalizarEmail(solicitud.Email);
        Usuario? usuarioExistente = await _usuarioRepositorio.ObtenerPorEmail(emailNormalizado, cancellationToken);

        if (usuarioExistente != null)
        {
            throw new BusinessRuleException("USR-001", $"El email '{emailNormalizado}' ya esta registrado.");
        }

        var usuario = new Usuario
        {
            Id = Guid.NewGuid().ToString(),
            Nombre = solicitud.Nombre.Trim(),
            Apellido = solicitud.Apellido.Trim(),
            Email = emailNormalizado,
            PasswordHash = CalcularSha256(solicitud.Password),
            FechaRegistro = DateTime.UtcNow,
            Activo = true,
            IntentosFallidos = 0,
            MotivoBloqueo = null
        };

        await _usuarioRepositorio.Crear(usuario, cancellationToken);

        _logger.LogInformation("Usuario registrado. UsuarioId: {UsuarioId}. Email: {Email}",
            usuario.Id,
            usuario.Email);

        return ConvertirARespuestaUsuario(usuario);
    }

    public async Task<RespuestaLogin> Login(SolicitudLogin solicitud, CancellationToken cancellationToken = default)
    {
        ValidarSolicitudLogin(solicitud);

        string emailNormalizado = NormalizarEmail(solicitud.Email);
        Usuario? usuario = await _usuarioRepositorio.ObtenerPorEmail(emailNormalizado, cancellationToken);

        if (usuario == null)
        {
            throw new UnauthorizedException("USR-003", "Credenciales incorrectas.");
        }

        if (!usuario.Activo)
        {
            LanzarErrorUsuarioBloqueado(usuario);
        }

        string passwordHash = CalcularSha256(solicitud.Password);

        if (!string.Equals(usuario.PasswordHash, passwordHash, StringComparison.OrdinalIgnoreCase))
        {
            int intentos = await _usuarioRepositorio.IncrementarIntentoFallido(emailNormalizado, cancellationToken);

            _logger.LogWarning("Login fallido. UsuarioId: {UsuarioId}. Email: {Email}. IntentosFallidos: {IntentosFallidos}",
                usuario.Id,
                usuario.Email,
                intentos);

            if (intentos >= MaximoIntentosFallidos)
            {
                await _usuarioRepositorio.BloquearPorIntentosFallidos(emailNormalizado, cancellationToken);

                throw new ForbiddenException(
                    "USR-004",
                    "Su cuenta fue bloqueada por superar el maximo de intentos fallidos. Contacte a soporte.");
            }

            throw new UnauthorizedException("USR-003", "Credenciales incorrectas.");
        }

        await _usuarioRepositorio.ReiniciarIntentosFallidos(emailNormalizado, cancellationToken);

        _logger.LogInformation("Login exitoso. UsuarioId: {UsuarioId}. Email: {Email}",
            usuario.Id,
            usuario.Email);

        return new RespuestaLogin
        {
            Id = usuario.Id,
            Nombre = usuario.Nombre,
            Apellido = usuario.Apellido,
            Email = usuario.Email
        };
    }

    private static void ValidarSolicitudRegistro(SolicitudRegistrarUsuario solicitud)
    {
        var errores = new List<string>();

        if (string.IsNullOrWhiteSpace(solicitud.Nombre))
        {
            errores.Add("El nombre es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(solicitud.Apellido))
        {
            errores.Add("El apellido es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(solicitud.Email))
        {
            errores.Add("El email es obligatorio.");
        }
        else if (!EmailTieneFormatoValido(solicitud.Email))
        {
            errores.Add("El email debe tener un formato valido.");
        }

        if (string.IsNullOrWhiteSpace(solicitud.Password))
        {
            errores.Add("La password es obligatoria.");
        }
        else if (solicitud.Password.Length < 8)
        {
            errores.Add("La password debe tener al menos 8 caracteres.");
        }

        if (errores.Count > 0)
        {
            throw new ValidationException("USR-002", string.Join("; ", errores));
        }
    }

    private static void ValidarSolicitudLogin(SolicitudLogin solicitud)
    {
        var errores = new List<string>();

        if (string.IsNullOrWhiteSpace(solicitud.Email))
        {
            errores.Add("El email es obligatorio.");
        }
        else if (!EmailTieneFormatoValido(solicitud.Email))
        {
            errores.Add("El email debe tener un formato valido.");
        }

        if (string.IsNullOrWhiteSpace(solicitud.Password))
        {
            errores.Add("La password es obligatoria.");
        }

        if (errores.Count > 0)
        {
            throw new ValidationException("USR-002", string.Join("; ", errores));
        }
    }

    private static void LanzarErrorUsuarioBloqueado(Usuario usuario)
    {
        if (string.Equals(usuario.MotivoBloqueo, "Fraude", StringComparison.OrdinalIgnoreCase))
        {
            throw new ForbiddenException(
                "USR-005",
                "Su cuenta fue suspendida por razones de seguridad. Contacte a soporte.");
        }

        throw new ForbiddenException(
            "USR-004",
            "Su cuenta fue bloqueada por superar el maximo de intentos fallidos. Contacte a soporte.");
    }

    private static RespuestaUsuario ConvertirARespuestaUsuario(Usuario usuario)
    {
        return new RespuestaUsuario
        {
            Id = usuario.Id,
            Nombre = usuario.Nombre,
            Apellido = usuario.Apellido,
            Email = usuario.Email,
            FechaRegistro = usuario.FechaRegistro,
            Activo = usuario.Activo
        };
    }

    private static string NormalizarEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    private static bool EmailTieneFormatoValido(string email)
    {
        return Regex.IsMatch(email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }

    private static string CalcularSha256(string texto)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(texto));
        return Convert.ToHexString(bytes);
    }
}
