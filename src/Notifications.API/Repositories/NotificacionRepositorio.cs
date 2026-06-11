using Dapper;
using Microsoft.Data.Sqlite;
using Notifications.API.Models;

namespace Notifications.API.Repositories;

public class NotificacionRepositorio : INotificacionRepositorio
{
    private readonly IConfiguration _configuration;

    public NotificacionRepositorio(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<IEnumerable<Notificacion>> ObtenerPorUsuarioId(
        string usuarioId,
        CancellationToken cancellationToken = default)
    {
        using var conexion = CrearConexion();
        await conexion.OpenAsync(cancellationToken);

        return await conexion.QueryAsync<Notificacion>("""
            SELECT
                id AS Id,
                usuario_id AS UsuarioId,
                mensaje AS Mensaje,
                tipo AS Tipo,
                estado AS Estado,
                fecha_envio AS FechaEnvio
            FROM notificaciones
            WHERE usuario_id = @UsuarioId
            ORDER BY fecha_envio DESC;
        """, new
        {
            UsuarioId = usuarioId.Trim()
        });
    }

    public async Task Crear(Notificacion notificacion, CancellationToken cancellationToken = default)
    {
        using var conexion = CrearConexion();
        await conexion.OpenAsync(cancellationToken);

        await conexion.ExecuteAsync("""
            INSERT INTO notificaciones (
                id,
                usuario_id,
                mensaje,
                tipo,
                estado,
                fecha_envio
            )
            VALUES (
                @Id,
                @UsuarioId,
                @Mensaje,
                @Tipo,
                @Estado,
                @FechaEnvio
            );
        """, new
        {
            notificacion.Id,
            notificacion.UsuarioId,
            notificacion.Mensaje,
            notificacion.Tipo,
            notificacion.Estado,
            FechaEnvio = notificacion.FechaEnvio.ToString("O")
        });
    }

    private SqliteConnection CrearConexion()
    {
        string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "Data Source=notifications.db";
        return new SqliteConnection(connectionString);
    }
}
