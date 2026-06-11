using Dapper;
using Microsoft.Data.Sqlite;

namespace Cart.API.Data;

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

        conexion.Execute("PRAGMA foreign_keys = ON;");

        conexion.Execute("""
            CREATE TABLE IF NOT EXISTS carritos (
                usuario_id TEXT PRIMARY KEY,
                fecha_actualizacion TEXT NOT NULL
            );
        """);

        conexion.Execute("""
            CREATE TABLE IF NOT EXISTS carrito_items (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                usuario_id TEXT NOT NULL,
                producto_id TEXT NOT NULL,
                cantidad INTEGER NOT NULL,
                FOREIGN KEY (usuario_id) REFERENCES carritos(usuario_id) ON DELETE CASCADE,
                UNIQUE (usuario_id, producto_id)
            );
        """);

        int cantidad = conexion.ExecuteScalar<int>("SELECT COUNT(1) FROM carritos;");

        if (cantidad == 0)
        {
            InsertarDatosIniciales(conexion);
        }

        _logger.LogInformation("Base de datos SQLite de Cart.API inicializada correctamente.");
    }

    private string ObtenerCadenaConexion()
    {
        return _configuration.GetConnectionString("DefaultConnection") ?? "Data Source=cart.db";
    }

    private static void InsertarDatosIniciales(SqliteConnection conexion)
    {
        const string usuarioId = "a1b2c3d4-0000-0000-0000-111122223333";
        const string productoId = "3fa85f64-5717-4562-b3fc-2c963f66afa6";

        conexion.Execute("""
            INSERT INTO carritos (
                usuario_id,
                fecha_actualizacion
            )
            VALUES (
                @UsuarioId,
                @FechaActualizacion
            );
        """, new
        {
            UsuarioId = usuarioId,
            FechaActualizacion = DateTime.UtcNow.ToString("O")
        });

        conexion.Execute("""
            INSERT INTO carrito_items (
                usuario_id,
                producto_id,
                cantidad
            )
            VALUES (
                @UsuarioId,
                @ProductoId,
                @Cantidad
            );
        """, new
        {
            UsuarioId = usuarioId,
            ProductoId = productoId,
            Cantidad = 1
        });
    }
}
