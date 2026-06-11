using Notifications.API.Clients;
using Notifications.API.DTOs;
using Notifications.API.Exceptions;
using Notifications.API.Models;
using Notifications.API.Repositories;
using ValidationException = Notifications.API.Exceptions.ValidationException;

namespace Notifications.API.Services;

public class NotificacionServicio : INotificacionServicio
{
    private static readonly HashSet<string> TiposValidos = new(StringComparer.OrdinalIgnoreCase)
    {
        "Email",
        "Push",
        "SMS"
    };

    private readonly INotificacionRepositorio _notificacionRepositorio;
    private readonly IUsuariosCliente _usuariosCliente;
    private readonly ILogger<NotificacionServicio> _logger;

    public NotificacionServicio(
        INotificacionRepositorio notificacionRepositorio,
        IUsuariosCliente usuariosCliente,
        ILogger<NotificacionServicio> logger)
    {
        _notificacionRepositorio = notificacionRepositorio;
        _usuariosCliente = usuariosCliente;
        _logger = logger;
    }

    public async Task<RespuestaNotificacion> Enviar(
        SolicitudEnviarNotificacion solicitud,
        CancellationToken cancellationToken = default)
    {
        ValidarSolicitud(solicitud);

        string usuarioId = solicitud.UsuarioId.Trim();
        bool usuarioExiste = await _usuariosCliente.ExisteUsuario(usuarioId, cancellationToken);

        if (!usuarioExiste)
        {
            throw new NotFoundException("NTF-001", "El usuario destinatario no fue encontrado.");
        }

        var notificacion = new Notificacion
        {
            Id = Guid.NewGuid().ToString(),
            UsuarioId = usuarioId,
            Mensaje = solicitud.Mensaje.Trim(),
            Tipo = NormalizarTipo(solicitud.Tipo),
            Estado = "Enviada",
            FechaEnvio = DateTime.UtcNow
        };

        await _notificacionRepositorio.Crear(notificacion, cancellationToken);

        _logger.LogInformation(
            "Notificacion registrada y enviada. NotificacionId: {NotificacionId}. UsuarioId: {UsuarioId}. Tipo: {Tipo}",
            notificacion.Id,
            notificacion.UsuarioId,
            notificacion.Tipo);

        return ConvertirARespuesta(notificacion);
    }

    public async Task<IEnumerable<RespuestaNotificacion>> ObtenerPorUsuarioId(
        string usuarioId,
        CancellationToken cancellationToken = default)
    {
        string usuarioIdNormalizado = NormalizarUsuarioId(usuarioId);

        IEnumerable<Notificacion> notificaciones = await _notificacionRepositorio.ObtenerPorUsuarioId(
            usuarioIdNormalizado,
            cancellationToken);

        List<Notificacion> lista = notificaciones.ToList();

        if (lista.Count == 0)
        {
            throw new NotFoundException("NTF-003", "No se encontraron notificaciones para el usuario.");
        }

        return lista.Select(ConvertirARespuesta);
    }

    private static void ValidarSolicitud(SolicitudEnviarNotificacion solicitud)
    {
        var errores = new List<string>();

        if (string.IsNullOrWhiteSpace(solicitud.UsuarioId))
        {
            errores.Add("El usuarioId es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(solicitud.Mensaje))
        {
            errores.Add("El mensaje es obligatorio.");
        }
        else if (solicitud.Mensaje.Trim().Length > 500)
        {
            errores.Add("El mensaje no puede superar los 500 caracteres.");
        }

        if (string.IsNullOrWhiteSpace(solicitud.Tipo))
        {
            errores.Add("El tipo de notificacion es obligatorio.");
        }
        else if (!TiposValidos.Contains(solicitud.Tipo.Trim()))
        {
            errores.Add("El tipo de notificacion debe ser Email, Push o SMS.");
        }

        if (errores.Count > 0)
        {
            throw new ValidationException("NTF-002", string.Join("; ", errores));
        }
    }

    private static string NormalizarUsuarioId(string usuarioId)
    {
        if (string.IsNullOrWhiteSpace(usuarioId))
        {
            throw new ValidationException("NTF-002", "El userId es obligatorio.");
        }

        return usuarioId.Trim();
    }

    private static string NormalizarTipo(string tipo)
    {
        string tipoNormalizado = tipo.Trim();

        if (tipoNormalizado.Equals("email", StringComparison.OrdinalIgnoreCase))
        {
            return "Email";
        }

        if (tipoNormalizado.Equals("push", StringComparison.OrdinalIgnoreCase))
        {
            return "Push";
        }

        if (tipoNormalizado.Equals("sms", StringComparison.OrdinalIgnoreCase))
        {
            return "SMS";
        }

        return tipoNormalizado;
    }

    private static RespuestaNotificacion ConvertirARespuesta(Notificacion notificacion)
    {
        return new RespuestaNotificacion
        {
            Id = notificacion.Id,
            UsuarioId = notificacion.UsuarioId,
            Mensaje = notificacion.Mensaje,
            Tipo = notificacion.Tipo,
            Estado = notificacion.Estado,
            FechaEnvio = notificacion.FechaEnvio
        };
    }
}
