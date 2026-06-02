using Products.API.DTOs;
using Products.API.Exceptions;
using Products.API.Models;
using Products.API.Repositories;
using ValidationException = Products.API.Exceptions.ValidationException;

namespace Products.API.Services;

public class ProductoServicio : IProductoServicio
{
    private readonly IProductoRepositorio _productoRepositorio;
    private readonly ILogger<ProductoServicio> _logger;

    public ProductoServicio(
        IProductoRepositorio productoRepositorio,
        ILogger<ProductoServicio> logger)
    {
        _productoRepositorio = productoRepositorio;
        _logger = logger;
    }

    public IEnumerable<RespuestaProducto> ObtenerTodos(string? categoria, string? nombre)
    {
        IEnumerable<Producto> productos = _productoRepositorio.ObtenerTodos(categoria, nombre);

        var respuestas = new List<RespuestaProducto>();

        foreach (Producto producto in productos)
        {
            respuestas.Add(ConvertirARespuesta(producto));
        }

        return respuestas;
    }

    public RespuestaProducto ObtenerPorId(string id)
    {
        Producto producto = ObtenerProductoExistente(id);

        return ConvertirARespuesta(producto);
    }

    public RespuestaProducto Crear(SolicitudCrearProducto solicitud)
    {
        ValidarSolicitud(solicitud.Nombre, solicitud.Descripcion, solicitud.Precio, solicitud.Stock, solicitud.Categoria);

        bool productoDuplicado = _productoRepositorio.ExisteProductoConNombreYCategoria(
            solicitud.Nombre,
            solicitud.Categoria);

        if (productoDuplicado)
        {
            throw new BusinessRuleException(
                "PRD-003",
                $"Ya existe un producto con ese nombre en la categoria '{solicitud.Categoria}'.");
        }

        var producto = new Producto
        {
            Id = Guid.NewGuid().ToString(),
            Nombre = solicitud.Nombre.Trim(),
            Descripcion = string.IsNullOrWhiteSpace(solicitud.Descripcion) ? null : solicitud.Descripcion.Trim(),
            Precio = solicitud.Precio,
            Stock = solicitud.Stock,
            Categoria = solicitud.Categoria.Trim(),
            FechaCreacion = DateTime.UtcNow
        };

        _productoRepositorio.Crear(producto);

        _logger.LogInformation("Producto creado. ProductoId: {ProductoId}", producto.Id);

        return ConvertirARespuesta(producto);
    }

    public RespuestaProducto Actualizar(string id, SolicitudActualizarProducto solicitud)
    {
        Producto producto = ObtenerProductoExistente(id);

        ValidarSolicitud(solicitud.Nombre, solicitud.Descripcion, solicitud.Precio, solicitud.Stock, solicitud.Categoria);

        bool productoDuplicado = _productoRepositorio.ExisteProductoConNombreYCategoriaExcluyendoId(
            id,
            solicitud.Nombre,
            solicitud.Categoria);

        if (productoDuplicado)
        {
            throw new BusinessRuleException(
                "PRD-003",
                $"Ya existe un producto con ese nombre en la categoria '{solicitud.Categoria}'.");
        }

        producto.Nombre = solicitud.Nombre.Trim();
        producto.Descripcion = string.IsNullOrWhiteSpace(solicitud.Descripcion) ? null : solicitud.Descripcion.Trim();
        producto.Precio = solicitud.Precio;
        producto.Stock = solicitud.Stock;
        producto.Categoria = solicitud.Categoria.Trim();

        _productoRepositorio.Actualizar(producto);

        _logger.LogInformation("Producto actualizado. ProductoId: {ProductoId}", producto.Id);

        return ConvertirARespuesta(producto);
    }

    public void Eliminar(string id)
    {
        Producto producto = ObtenerProductoExistente(id);

        bool tieneOrdenesActivas = _productoRepositorio.TieneOrdenesActivas(id);

        if (tieneOrdenesActivas)
        {
            throw new BusinessRuleException(
                "PRD-004",
                "El producto tiene ordenes activas y no puede eliminarse.");
        }

        _productoRepositorio.Eliminar(id);

        _logger.LogInformation("Producto eliminado. ProductoId: {ProductoId}", producto.Id);
    }

    private Producto ObtenerProductoExistente(string id)
    {
        Producto? producto = _productoRepositorio.ObtenerPorId(id);

        if (producto == null)
        {
            throw new NotFoundException("PRD-001", "Producto no encontrado.");
        }

        return producto;
    }

    private static void ValidarSolicitud(
        string nombre,
        string? descripcion,
        decimal precio,
        int stock,
        string categoria)
    {
        var errores = new List<string>();

        if (string.IsNullOrWhiteSpace(nombre))
        {
            errores.Add("El nombre del producto es obligatorio.");
        }
        else if (nombre.Trim().Length > 100)
        {
            errores.Add("El nombre no puede superar los 100 caracteres.");
        }

        if (!string.IsNullOrWhiteSpace(descripcion) && descripcion.Trim().Length > 500)
        {
            errores.Add("La descripcion no puede superar los 500 caracteres.");
        }

        if (precio <= 0)
        {
            errores.Add("El precio debe ser mayor a cero.");
        }

        if (stock < 0)
        {
            errores.Add("El stock no puede ser negativo.");
        }

        if (string.IsNullOrWhiteSpace(categoria))
        {
            errores.Add("La categoria es obligatoria.");
        }

        if (errores.Count > 0)
        {
            throw new ValidationException("PRD-002", string.Join("; ", errores));
        }
    }

    private static RespuestaProducto ConvertirARespuesta(Producto producto)
    {
        return new RespuestaProducto
        {
            Id = producto.Id,
            Nombre = producto.Nombre,
            Descripcion = producto.Descripcion,
            Precio = producto.Precio,
            Stock = producto.Stock,
            Categoria = producto.Categoria,
            FechaCreacion = producto.FechaCreacion
        };
    }
}
