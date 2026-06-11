using Dapper;
using Microsoft.Data.Sqlite;
using Orders.API.Models;

namespace Orders.API.Repositories;

public class OrdenRepositorio : IOrdenRepositorio
{
    private readonly IConfiguration _configuration;

    public OrdenRepositorio(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<IEnumerable<Orden>> ObtenerTodos(string? usuarioId, CancellationToken cancellationToken = default)
    {
        using var conexion = CrearConexion();
        await conexion.OpenAsync(cancellationToken);

        string consulta = """
            SELECT
                id AS Id,
                usuario_id AS UsuarioId,
                total AS Total,
                estado AS Estado,
                fecha_creacion AS FechaCreacion,
                fecha_actualizacion AS FechaActualizacion
            FROM ordenes
            WHERE (@UsuarioId IS NULL OR usuario_id = @UsuarioId)
            ORDER BY fecha_creacion DESC;
        """;

        IEnumerable<Orden> ordenes = await conexion.QueryAsync<Orden>(consulta, new
        {
            UsuarioId = string.IsNullOrWhiteSpace(usuarioId) ? null : usuarioId.Trim()
        });

        var resultado = new List<Orden>();

        foreach (Orden orden in ordenes)
        {
            orden.Items = (await ObtenerItems(conexion, orden.Id)).ToList();
            resultado.Add(orden);
        }

        return resultado;
    }

    public async Task<Orden?> ObtenerPorId(string id, CancellationToken cancellationToken = default)
    {
        using var conexion = CrearConexion();
        await conexion.OpenAsync(cancellationToken);

        string consulta = """
            SELECT
                id AS Id,
                usuario_id AS UsuarioId,
                total AS Total,
                estado AS Estado,
                fecha_creacion AS FechaCreacion,
                fecha_actualizacion AS FechaActualizacion
            FROM ordenes
            WHERE id = @Id;
        """;

        Orden? orden = await conexion.QuerySingleOrDefaultAsync<Orden>(consulta, new
        {
            Id = id
        });

        if (orden == null)
        {
            return null;
        }

        orden.Items = (await ObtenerItems(conexion, orden.Id)).ToList();
        return orden;
    }

    public async Task Crear(Orden orden, CancellationToken cancellationToken = default)
    {
        using var conexion = CrearConexion();
        await conexion.OpenAsync(cancellationToken);

        using var transaccion = conexion.BeginTransaction();

        await conexion.ExecuteAsync("""
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
                @FechaActualizacion
            );
        """, new
        {
            orden.Id,
            orden.UsuarioId,
            orden.Total,
            orden.Estado,
            FechaCreacion = orden.FechaCreacion.ToString("O"),
            FechaActualizacion = orden.FechaActualizacion?.ToString("O")
        }, transaccion);

        foreach (OrdenItem item in orden.Items)
        {
            await conexion.ExecuteAsync("""
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
                item.OrdenId,
                item.ProductoId,
                item.Cantidad,
                item.PrecioUnitario
            }, transaccion);
        }

        transaccion.Commit();
    }

    public async Task ActualizarEstado(string id, string estado, DateTime fechaActualizacion, CancellationToken cancellationToken = default)
    {
        using var conexion = CrearConexion();
        await conexion.OpenAsync(cancellationToken);

        await conexion.ExecuteAsync("""
            UPDATE ordenes
            SET estado = @Estado,
                fecha_actualizacion = @FechaActualizacion
            WHERE id = @Id;
        """, new
        {
            Id = id,
            Estado = estado,
            FechaActualizacion = fechaActualizacion.ToString("O")
        });
    }

    private SqliteConnection CrearConexion()
    {
        string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "Data Source=orders.db";
        return new SqliteConnection(connectionString);
    }

    private static async Task<IEnumerable<OrdenItem>> ObtenerItems(SqliteConnection conexion, string ordenId)
    {
        string consulta = """
            SELECT
                orden_id AS OrdenId,
                producto_id AS ProductoId,
                cantidad AS Cantidad,
                precio_unitario AS PrecioUnitario
            FROM orden_items
            WHERE orden_id = @OrdenId
            ORDER BY id;
        """;

        return await conexion.QueryAsync<OrdenItem>(consulta, new
        {
            OrdenId = ordenId
        });
    }
}
