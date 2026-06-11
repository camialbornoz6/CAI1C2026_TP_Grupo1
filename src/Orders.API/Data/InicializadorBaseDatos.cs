using Dapper;
using Microsoft.Data.Sqlite;

namespace Orders.API.Data;

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
            CREATE TABLE IF NOT EXISTS ordenes (
                id TEXT PRIMARY KEY,
                usuario_id TEXT NOT NULL,
                total REAL NOT NULL,
                estado TEXT NOT NULL,
                fecha_creacion TEXT NOT NULL,
                fecha_actualizacion TEXT NULL
            );
        """);

        conexion.Execute("""
            CREATE TABLE IF NOT EXISTS orden_items (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                orden_id TEXT NOT NULL,
                producto_id TEXT NOT NULL,
                cantidad INTEGER NOT NULL,
                precio_unitario REAL NOT NULL,
                FOREIGN KEY (orden_id) REFERENCES ordenes(id)
            );
        """);

        int cantidad = conexion.ExecuteScalar<int>("SELECT COUNT(1) FROM ordenes;");

        if (cantidad == 0)
        {
            InsertarDatosIniciales(conexion);
        }

        _logger.LogInformation("Base de datos SQLite de Orders.API inicializada correctamente.");
    }

    private string ObtenerCadenaConexion()
    {
        return _configuration.GetConnectionString("DefaultConnection") ?? "Data Source=orders.db";
    }

    private static void InsertarDatosIniciales(SqliteConnection conexion)
    {
        const string ordenId = "11112222-3333-4444-5555-666677778888";
        const string usuarioId = "a1b2c3d4-0000-0000-0000-111122223333";
        const string productoId = "3fa85f64-5717-4562-b3fc-2c963f66afa6";
        const decimal precioUnitario = 1500.00m;
        const int cantidad = 1;
        const decimal total = precioUnitario * cantidad;

        conexion.Execute("""
            INSERT INTO ordenes (
                id,
                usuario_id,
                total,
                estado,
                fecha_creacion,
                fecha_actualizacion
            )
            VALUES (
                @Id,
                @UsuarioId,
                @Total,
                @Estado,
                @FechaCreacion,
                NULL
            );
        """, new
        {
            Id = ordenId,
            UsuarioId = usuarioId,
            Total = total,
            Estado = "Pendiente",
            FechaCreacion = DateTime.UtcNow.ToString("O")
        });

        conexion.Execute("""
            INSERT INTO orden_items (
                orden_id,
                producto_id,
                cantidad,
                precio_unitario
            )
            VALUES (
                @OrdenId,
                @ProductoId,
                @Cantidad,
                @PrecioUnitario
            );
        """, new
        {
            OrdenId = ordenId,
            ProductoId = productoId,
            Cantidad = cantidad,
            PrecioUnitario = precioUnitario
        });
    }
}
