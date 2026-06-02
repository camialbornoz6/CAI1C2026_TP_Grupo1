using Dapper;
using Microsoft.Data.Sqlite;
using Products.API.Models;

namespace Products.API.Repositories;

public class ProductoRepositorio : IProductoRepositorio
{
    private readonly IConfiguration _configuracion;

    public ProductoRepositorio(IConfiguration configuracion)
    {
        _configuracion = configuracion;
    }

    private SqliteConnection CrearConexion()
    {
        string cadenaConexion = _configuracion.GetConnectionString("DefaultConnection")
            ?? "Data Source=products.db";

        return new SqliteConnection(cadenaConexion);
    }

    public IEnumerable<Producto> ObtenerTodos(string? categoria, string? nombre)
    {
        using var conexion = CrearConexion();

        string consulta = """
            SELECT
                id AS Id,
                nombre AS Nombre,
                descripcion AS Descripcion,
                precio AS Precio,
                stock AS Stock,
                categoria AS Categoria,
                fecha_creacion AS FechaCreacion
            FROM productos
            WHERE (@Categoria IS NULL OR lower(categoria) = lower(@Categoria))
              AND (@Nombre IS NULL OR lower(nombre) LIKE '%' || lower(@Nombre) || '%')
            ORDER BY nombre;
        """;

        return conexion.Query<Producto>(consulta, new
        {
            Categoria = string.IsNullOrWhiteSpace(categoria) ? null : categoria.Trim(),
            Nombre = string.IsNullOrWhiteSpace(nombre) ? null : nombre.Trim()
        });
    }

    public Producto? ObtenerPorId(string id)
    {
        using var conexion = CrearConexion();

        string consulta = """
            SELECT
                id AS Id,
                nombre AS Nombre,
                descripcion AS Descripcion,
                precio AS Precio,
                stock AS Stock,
                categoria AS Categoria,
                fecha_creacion AS FechaCreacion
            FROM productos
            WHERE id = @Id;
        """;

        return conexion.QuerySingleOrDefault<Producto>(consulta, new
        {
            Id = id
        });
    }

    public bool ExisteProductoConNombreYCategoria(string nombre, string categoria)
    {
        using var conexion = CrearConexion();

        string consulta = """
            SELECT COUNT(1)
            FROM productos
            WHERE lower(nombre) = lower(@Nombre)
              AND lower(categoria) = lower(@Categoria);
        """;

        int cantidad = conexion.ExecuteScalar<int>(consulta, new
        {
            Nombre = nombre.Trim(),
            Categoria = categoria.Trim()
        });

        return cantidad > 0;
    }

    public bool ExisteProductoConNombreYCategoriaExcluyendoId(string id, string nombre, string categoria)
    {
        using var conexion = CrearConexion();

        string consulta = """
            SELECT COUNT(1)
            FROM productos
            WHERE id <> @Id
              AND lower(nombre) = lower(@Nombre)
              AND lower(categoria) = lower(@Categoria);
        """;

        int cantidad = conexion.ExecuteScalar<int>(consulta, new
        {
            Id = id,
            Nombre = nombre.Trim(),
            Categoria = categoria.Trim()
        });

        return cantidad > 0;
    }

    public void Crear(Producto producto)
    {
        using var conexion = CrearConexion();

        string sentencia = """
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
                @Id,
                @Nombre,
                @Descripcion,
                @Precio,
                @Stock,
                @Categoria,
                @FechaCreacion
            );
        """;

        conexion.Execute(sentencia, new
        {
            producto.Id,
            producto.Nombre,
            producto.Descripcion,
            producto.Precio,
            producto.Stock,
            producto.Categoria,
            FechaCreacion = producto.FechaCreacion.ToString("O")
        });
    }

    public void Actualizar(Producto producto)
    {
        using var conexion = CrearConexion();

        string sentencia = """
            UPDATE productos
            SET nombre = @Nombre,
                descripcion = @Descripcion,
                precio = @Precio,
                stock = @Stock,
                categoria = @Categoria
            WHERE id = @Id;
        """;

        conexion.Execute(sentencia, new
        {
            producto.Id,
            producto.Nombre,
            producto.Descripcion,
            producto.Precio,
            producto.Stock,
            producto.Categoria
        });
    }

    public void Eliminar(string id)
    {
        using var conexion = CrearConexion();

        conexion.Execute("DELETE FROM productos WHERE id = @Id;", new
        {
            Id = id
        });
    }

    public bool TieneOrdenesActivas(string id)
    {
        using var conexion = CrearConexion();

        string consulta = """
            SELECT COUNT(1)
            FROM ordenes_activas_productos
            WHERE producto_id = @Id
              AND estado IN ('Pendiente', 'Confirmada');
        """;

        int cantidad = conexion.ExecuteScalar<int>(consulta, new
        {
            Id = id
        });

        return cantidad > 0;
    }
}
