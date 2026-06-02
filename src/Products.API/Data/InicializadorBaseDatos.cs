using Dapper;
using Microsoft.Data.Sqlite;

namespace Products.API.Data;

public class InicializadorBaseDatos
{
    private readonly IConfiguration _configuracion;
    private readonly ILogger<InicializadorBaseDatos> _logger;

    public InicializadorBaseDatos(
        IConfiguration configuracion,
        ILogger<InicializadorBaseDatos> logger)
    {
        _configuracion = configuracion;
        _logger = logger;
    }

    public void Inicializar()
    {
        string cadenaConexion = _configuracion.GetConnectionString("DefaultConnection")
            ?? "Data Source=products.db";

        using var conexion = new SqliteConnection(cadenaConexion);

        conexion.Open();

        conexion.Execute("""
            CREATE TABLE IF NOT EXISTS productos (
                id TEXT PRIMARY KEY,
                nombre TEXT NOT NULL,
                descripcion TEXT NULL,
                precio REAL NOT NULL,
                stock INTEGER NOT NULL,
                categoria TEXT NOT NULL,
                fecha_creacion TEXT NOT NULL
            );
        """);

        // Tabla minima para simular el bloqueo de borrado por ordenes activas
        // mientras Orders.API todavia no esta integrada.
        conexion.Execute("""
            CREATE TABLE IF NOT EXISTS ordenes_activas_productos (
                producto_id TEXT NOT NULL,
                estado TEXT NOT NULL
            );
        """);

        int cantidadProductos = conexion.ExecuteScalar<int>("SELECT COUNT(1) FROM productos;");

        if (cantidadProductos == 0)
        {
            conexion.Execute("""
                INSERT INTO productos (
                    id,
                    nombre,
                    descripcion,
                    precio,
                    stock,
                    categoria,
                    fecha_creacion
                )
                VALUES (
                    '3fa85f64-5717-4562-b3fc-2c963f66afa6',
                    'Notebook Dell XPS 15',
                    'Laptop 15 pulgadas, 32GB RAM',
                    1500.00,
                    10,
                    'Electronica',
                    datetime('now')
                );
            """);

            conexion.Execute("""
                INSERT INTO productos (
                    id,
                    nombre,
                    descripcion,
                    precio,
                    stock,
                    categoria,
                    fecha_creacion
                )
                VALUES (
                    'aaaabbbb-cccc-dddd-eeee-ffff00001111',
                    'Mouse Logitech',
                    'Mouse inalambrico',
                    25.50,
                    30,
                    'Electronica',
                    datetime('now')
                );
            """);
        }

        int cantidadOrdenesSimuladas = conexion.ExecuteScalar<int>("SELECT COUNT(1) FROM ordenes_activas_productos;");

        if (cantidadOrdenesSimuladas == 0)
        {
            conexion.Execute("""
                INSERT INTO ordenes_activas_productos (producto_id, estado)
                VALUES ('3fa85f64-5717-4562-b3fc-2c963f66afa6', 'Pendiente');
            """);
        }

        _logger.LogInformation("Base SQLite inicializada correctamente.");
    }
}
