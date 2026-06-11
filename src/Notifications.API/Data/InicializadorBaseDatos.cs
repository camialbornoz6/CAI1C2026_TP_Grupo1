using Dapper;
using Microsoft.Data.Sqlite;

namespace Notifications.API.Data;

public class InicializadorBaseDatos
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<InicializadorBaseDatos> _logger;

    public InicializadorBaseDatos(
        IConfiguration configuration,
        ILogger<InicializadorBaseDatos> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public void Inicializar()
    {
        string connectionString = ObtenerCadenaConexion();

        using var conexion = new SqliteConnection(connectionString);
        conexion.Open();

        conexion.Execute("""
            CREATE TABLE IF NOT EXISTS notificaciones (
                id TEXT PRIMARY KEY,
                usuario_id TEXT NOT NULL,
                mensaje TEXT NOT NULL,
                tipo TEXT NOT NULL,
                estado TEXT NOT NULL,
                fecha_envio TEXT NOT NULL
            );
        """);

        int cantidad = conexion.ExecuteScalar<int>("SELECT COUNT(1) FROM notificaciones;");

        if (cantidad == 0)
        {
            InsertarDatosIniciales(conexion);
        }

        _logger.LogInformation("Base de datos SQLite de Notifications.API inicializada correctamente.");
    }

    private string ObtenerCadenaConexion()
    {
        return _configuration.GetConnectionString("DefaultConnection") ?? "Data Source=notifications.db";
    }

    private static void InsertarDatosIniciales(SqliteConnection conexion)
    {
        conexion.Execute("""
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
            Id = "11112222-3333-4444-5555-666677778888",
            UsuarioId = "a1b2c3d4-0000-0000-0000-111122223333",
            Mensaje = "Su orden fue confirmada.",
            Tipo = "Email",
            Estado = "Enviada",
            FechaEnvio = "2024-03-10T12:01:00Z"
        });
    }
}
