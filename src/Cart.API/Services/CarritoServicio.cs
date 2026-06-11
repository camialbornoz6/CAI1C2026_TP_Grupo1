using Cart.API.Clients;
using Cart.API.DTOs;
using Cart.API.Exceptions;
using Cart.API.Models;
using Cart.API.Repositories;
using ValidationException = Cart.API.Exceptions.ValidationException;

namespace Cart.API.Services;

public class CarritoServicio : ICarritoServicio
{
    private readonly ICarritoRepositorio _carritoRepositorio;
    private readonly IProductosCliente _productosCliente;
    private readonly ILogger<CarritoServicio> _logger;

    public CarritoServicio(
        ICarritoRepositorio carritoRepositorio,
        IProductosCliente productosCliente,
        ILogger<CarritoServicio> logger)
    {
        _carritoRepositorio = carritoRepositorio;
        _productosCliente = productosCliente;
        _logger = logger;
    }

    public async Task<RespuestaCarrito> ObtenerPorUsuarioId(string usuarioId, CancellationToken cancellationToken = default)
    {
        string usuarioIdNormalizado = NormalizarUsuarioId(usuarioId);
        Carrito carrito = await ObtenerCarritoExistente(usuarioIdNormalizado, cancellationToken);

        return ConvertirARespuesta(carrito);
    }

    public async Task<RespuestaCarrito> AgregarItem(
        string usuarioId,
        SolicitudAgregarItemCarrito solicitud,
        CancellationToken cancellationToken = default)
    {
        string usuarioIdNormalizado = NormalizarUsuarioId(usuarioId);
        ValidarSolicitudAgregar(solicitud);

        string productoId = solicitud.ProductoId.Trim();
        ProductoExterno producto = await ObtenerProductoExistente(productoId, cancellationToken);

        Carrito? carritoActual = await _carritoRepositorio.ObtenerPorUsuarioId(usuarioIdNormalizado, cancellationToken);
        int cantidadActual = carritoActual?.Items
            .FirstOrDefault(i => i.ProductoId.Equals(productoId, StringComparison.OrdinalIgnoreCase))
            ?.Cantidad ?? 0;

        int cantidadFinal = cantidadActual + solicitud.Cantidad;
        ValidarStock(producto, cantidadFinal);

        DateTime fechaActualizacion = DateTime.UtcNow;
        await _carritoRepositorio.GuardarItem(usuarioIdNormalizado, producto.Id, cantidadFinal, fechaActualizacion, cancellationToken);

        _logger.LogInformation(
            "Item agregado al carrito. UsuarioId: {UsuarioId}. ProductoId: {ProductoId}. CantidadFinal: {CantidadFinal}",
            usuarioIdNormalizado,
            producto.Id,
            cantidadFinal);

        Carrito carritoActualizado = await ObtenerCarritoExistente(usuarioIdNormalizado, cancellationToken);
        return ConvertirARespuesta(carritoActualizado);
    }

    public async Task<RespuestaCarrito> ActualizarItem(
        string usuarioId,
        string productoId,
        SolicitudActualizarItemCarrito solicitud,
        CancellationToken cancellationToken = default)
    {
        string usuarioIdNormalizado = NormalizarUsuarioId(usuarioId);
        string productoIdNormalizado = NormalizarProductoId(productoId);
        ValidarSolicitudActualizar(solicitud);

        Carrito carrito = await ObtenerCarritoExistente(usuarioIdNormalizado, cancellationToken);
        CarritoItem? itemExistente = carrito.Items.FirstOrDefault(i =>
            i.ProductoId.Equals(productoIdNormalizado, StringComparison.OrdinalIgnoreCase));

        if (itemExistente == null)
        {
            throw new NotFoundException("CRT-001", "Carrito no encontrado o producto inexistente en el carrito.");
        }

        ProductoExterno producto = await ObtenerProductoExistente(productoIdNormalizado, cancellationToken);
        ValidarStock(producto, solicitud.Cantidad);

        DateTime fechaActualizacion = DateTime.UtcNow;
        await _carritoRepositorio.ActualizarItem(usuarioIdNormalizado, producto.Id, solicitud.Cantidad, fechaActualizacion, cancellationToken);

        _logger.LogInformation(
            "Cantidad de item actualizada. UsuarioId: {UsuarioId}. ProductoId: {ProductoId}. Cantidad: {Cantidad}",
            usuarioIdNormalizado,
            producto.Id,
            solicitud.Cantidad);

        Carrito carritoActualizado = await ObtenerCarritoExistente(usuarioIdNormalizado, cancellationToken);
        return ConvertirARespuesta(carritoActualizado);
    }

    public async Task EliminarItem(string usuarioId, string productoId, CancellationToken cancellationToken = default)
    {
        string usuarioIdNormalizado = NormalizarUsuarioId(usuarioId);
        string productoIdNormalizado = NormalizarProductoId(productoId);

        Carrito carrito = await ObtenerCarritoExistente(usuarioIdNormalizado, cancellationToken);
        bool existeItem = carrito.Items.Any(i => i.ProductoId.Equals(productoIdNormalizado, StringComparison.OrdinalIgnoreCase));

        if (!existeItem)
        {
            throw new NotFoundException("CRT-001", "Carrito no encontrado o producto inexistente en el carrito.");
        }

        await _carritoRepositorio.EliminarItem(usuarioIdNormalizado, productoIdNormalizado, DateTime.UtcNow, cancellationToken);

        _logger.LogInformation(
            "Item eliminado del carrito. UsuarioId: {UsuarioId}. ProductoId: {ProductoId}",
            usuarioIdNormalizado,
            productoIdNormalizado);
    }

    public async Task EliminarCarrito(string usuarioId, CancellationToken cancellationToken = default)
    {
        string usuarioIdNormalizado = NormalizarUsuarioId(usuarioId);
        await ObtenerCarritoExistente(usuarioIdNormalizado, cancellationToken);

        await _carritoRepositorio.EliminarCarrito(usuarioIdNormalizado, cancellationToken);

        _logger.LogInformation("Carrito eliminado. UsuarioId: {UsuarioId}", usuarioIdNormalizado);
    }

    private async Task<Carrito> ObtenerCarritoExistente(string usuarioId, CancellationToken cancellationToken)
    {
        Carrito? carrito = await _carritoRepositorio.ObtenerPorUsuarioId(usuarioId, cancellationToken);

        if (carrito == null)
        {
            throw new NotFoundException("CRT-001", "Carrito no encontrado.");
        }

        return carrito;
    }

    private async Task<ProductoExterno> ObtenerProductoExistente(string productoId, CancellationToken cancellationToken)
    {
        ProductoExterno? producto = await _productosCliente.ObtenerPorId(productoId, cancellationToken);

        if (producto == null)
        {
            throw new NotFoundException("CRT-002", "Producto no encontrado.");
        }

        return producto;
    }

    private static void ValidarStock(ProductoExterno producto, int cantidadSolicitada)
    {
        if (producto.Stock < cantidadSolicitada)
        {
            throw new BusinessRuleException(
                "CRT-003",
                $"Stock insuficiente. Disponible: {producto.Stock}, solicitado: {cantidadSolicitada}.");
        }
    }

    private static string NormalizarUsuarioId(string usuarioId)
    {
        if (string.IsNullOrWhiteSpace(usuarioId))
        {
            throw new ValidationException("CRT-004", "El userId es obligatorio.");
        }

        return usuarioId.Trim();
    }

    private static string NormalizarProductoId(string productoId)
    {
        if (string.IsNullOrWhiteSpace(productoId))
        {
            throw new ValidationException("CRT-004", "El productId es obligatorio.");
        }

        return productoId.Trim();
    }

    private static void ValidarSolicitudAgregar(SolicitudAgregarItemCarrito solicitud)
    {
        var errores = new List<string>();

        if (string.IsNullOrWhiteSpace(solicitud.ProductoId))
        {
            errores.Add("El productoId es obligatorio.");
        }

        if (solicitud.Cantidad <= 0)
        {
            errores.Add("La cantidad debe ser mayor a cero.");
        }

        if (errores.Count > 0)
        {
            throw new ValidationException("CRT-004", string.Join("; ", errores));
        }
    }

    private static void ValidarSolicitudActualizar(SolicitudActualizarItemCarrito solicitud)
    {
        if (solicitud.Cantidad <= 0)
        {
            throw new ValidationException("CRT-004", "La cantidad debe ser mayor a cero.");
        }
    }

    private static RespuestaCarrito ConvertirARespuesta(Carrito carrito)
    {
        return new RespuestaCarrito
        {
            UsuarioId = carrito.UsuarioId,
            FechaActualizacion = carrito.FechaActualizacion,
            Items = carrito.Items.Select(i => new RespuestaCarritoItem
            {
                ProductoId = i.ProductoId,
                Cantidad = i.Cantidad
            }).ToList()
        };
    }
}
