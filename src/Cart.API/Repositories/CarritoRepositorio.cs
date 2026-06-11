using Cart.API.Models;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Cart.API.Repositories;

public class CarritoRepositorio : ICarritoRepositorio
{
    private readonly IConfiguration _configuration;

    public CarritoRepositorio(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<Carrito?> ObtenerPorUsuarioId(string usuarioId, CancellationToken cancellationToken = default)
    {
        using var conexion = CrearConexion();
        await conexion.OpenAsync(cancellationToken);

        string consultaCarrito = """
            SELECT
                usuario_id AS UsuarioId,
                fecha_actualizacion AS FechaActualizacion
            FROM carritos
            WHERE usuario_id = @UsuarioId;
        """;

        Carrito? carrito = await conexion.QuerySingleOrDefaultAsync<Carrito>(consultaCarrito, new
        {
            UsuarioId = usuarioId
        });

        if (carrito == null)
        {
            return null;
        }

        carrito.Items = (await ObtenerItems(conexion, usuarioId)).ToList();
        return carrito;
    }

    public async Task GuardarItem(
        string usuarioId,
        string productoId,
        int cantidad,
        DateTime fechaActualizacion,
        CancellationToken cancellationToken = default)
    {
        using var conexion = CrearConexion();
        await conexion.OpenAsync(cancellationToken);

        using var transaccion = conexion.BeginTransaction();

        await conexion.ExecuteAsync("""
            INSERT INTO carritos (
                usuario_id,
                fecha_actualizacion
            )
            VALUES (
                @UsuarioId,
                @FechaActualizacion
            )
            ON CONFLICT(usuario_id) DO UPDATE SET
                fecha_actualizacion = excluded.fecha_actualizacion;
        """, new
        {
            UsuarioId = usuarioId,
            FechaActualizacion = fechaActualizacion.ToString("O")
        }, transaccion);

        await conexion.ExecuteAsync("""
            INSERT INTO carrito_items (
                usuario_id,
                producto_id,
                cantidad
            )
            VALUES (
                @UsuarioId,
                @ProductoId,
                @Cantidad
            )
            ON CONFLICT(usuario_id, producto_id) DO UPDATE SET
                cantidad = excluded.cantidad;
        """, new
        {
            UsuarioId = usuarioId,
            ProductoId = productoId,
            Cantidad = cantidad
        }, transaccion);

        transaccion.Commit();
    }

    public async Task ActualizarItem(
        string usuarioId,
        string productoId,
        int cantidad,
        DateTime fechaActualizacion,
        CancellationToken cancellationToken = default)
    {
        using var conexion = CrearConexion();
        await conexion.OpenAsync(cancellationToken);

        using var transaccion = conexion.BeginTransaction();

        await conexion.ExecuteAsync("""
            UPDATE carrito_items
            SET cantidad = @Cantidad
            WHERE usuario_id = @UsuarioId
              AND producto_id = @ProductoId;
        """, new
        {
            UsuarioId = usuarioId,
            ProductoId = productoId,
            Cantidad = cantidad
        }, transaccion);

        await conexion.ExecuteAsync("""
            UPDATE carritos
            SET fecha_actualizacion = @FechaActualizacion
            WHERE usuario_id = @UsuarioId;
        """, new
        {
            UsuarioId = usuarioId,
            FechaActualizacion = fechaActualizacion.ToString("O")
        }, transaccion);

        transaccion.Commit();
    }

    public async Task EliminarItem(
        string usuarioId,
        string productoId,
        DateTime fechaActualizacion,
        CancellationToken cancellationToken = default)
    {
        using var conexion = CrearConexion();
        await conexion.OpenAsync(cancellationToken);

        using var transaccion = conexion.BeginTransaction();

        await conexion.ExecuteAsync("""
            DELETE FROM carrito_items
            WHERE usuario_id = @UsuarioId
              AND producto_id = @ProductoId;
        """, new
        {
            UsuarioId = usuarioId,
            ProductoId = productoId
        }, transaccion);

        await conexion.ExecuteAsync("""
            UPDATE carritos
            SET fecha_actualizacion = @FechaActualizacion
            WHERE usuario_id = @UsuarioId;
        """, new
        {
            UsuarioId = usuarioId,
            FechaActualizacion = fechaActualizacion.ToString("O")
        }, transaccion);

        transaccion.Commit();
    }

    public async Task EliminarCarrito(string usuarioId, CancellationToken cancellationToken = default)
    {
        using var conexion = CrearConexion();
        await conexion.OpenAsync(cancellationToken);

        using var transaccion = conexion.BeginTransaction();

        await conexion.ExecuteAsync("""
            DELETE FROM carrito_items
            WHERE usuario_id = @UsuarioId;
        """, new
        {
            UsuarioId = usuarioId
        }, transaccion);

        await conexion.ExecuteAsync("""
            DELETE FROM carritos
            WHERE usuario_id = @UsuarioId;
        """, new
        {
            UsuarioId = usuarioId
        }, transaccion);

        transaccion.Commit();
    }

    private SqliteConnection CrearConexion()
    {
        string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "Data Source=cart.db";
        return new SqliteConnection(connectionString);
    }

    private static async Task<IEnumerable<CarritoItem>> ObtenerItems(SqliteConnection conexion, string usuarioId)
    {
        string consulta = """
            SELECT
                usuario_id AS UsuarioId,
                producto_id AS ProductoId,
                cantidad AS Cantidad
            FROM carrito_items
            WHERE usuario_id = @UsuarioId
            ORDER BY id;
        """;

        return await conexion.QueryAsync<CarritoItem>(consulta, new
        {
            UsuarioId = usuarioId
        });
    }
}
