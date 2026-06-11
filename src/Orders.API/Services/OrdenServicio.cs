using Orders.API.Clients;
using Orders.API.DTOs;
using Orders.API.Exceptions;
using Orders.API.Models;
using Orders.API.Repositories;
using ValidationException = Orders.API.Exceptions.ValidationException;

namespace Orders.API.Services;

public class OrdenServicio : IOrdenServicio
{
    private static readonly string[] EstadosValidos =
    {
        "Pendiente",
        "Confirmada",
        "Enviada",
        "Entregada",
        "Cancelada"
    };

    private readonly IOrdenRepositorio _ordenRepositorio;
    private readonly IUsuariosCliente _usuariosCliente;
    private readonly IProductosCliente _productosCliente;
    private readonly ILogger<OrdenServicio> _logger;

    public OrdenServicio(
        IOrdenRepositorio ordenRepositorio,
        IUsuariosCliente usuariosCliente,
        IProductosCliente productosCliente,
        ILogger<OrdenServicio> logger)
    {
        _ordenRepositorio = ordenRepositorio;
        _usuariosCliente = usuariosCliente;
        _productosCliente = productosCliente;
        _logger = logger;
    }

    public async Task<IEnumerable<RespuestaOrden>> ObtenerTodos(string? usuarioId, CancellationToken cancellationToken = default)
    {
        IEnumerable<Orden> ordenes = await _ordenRepositorio.ObtenerTodos(usuarioId, cancellationToken);

        var respuestas = new List<RespuestaOrden>();

        foreach (Orden orden in ordenes)
        {
            respuestas.Add(ConvertirARespuesta(orden));
        }

        return respuestas;
    }

    public async Task<RespuestaOrden> ObtenerPorId(string id, CancellationToken cancellationToken = default)
    {
        Orden orden = await ObtenerOrdenExistente(id, cancellationToken);

        return ConvertirARespuesta(orden);
    }

    public async Task<RespuestaOrden> Crear(SolicitudCrearOrden solicitud, CancellationToken cancellationToken = default)
    {
        ValidarSolicitudCrear(solicitud);

        bool usuarioExiste = await _usuariosCliente.ExisteUsuario(solicitud.UsuarioId.Trim(), cancellationToken);

        if (!usuarioExiste)
        {
            throw new NotFoundException("ORD-003", "Usuario no encontrado al crear la orden.");
        }

        List<SolicitudCrearOrdenItem> itemsAgrupados = AgruparItems(solicitud.Items);
        var itemsOrden = new List<OrdenItem>();
        decimal total = 0;

        foreach (SolicitudCrearOrdenItem itemSolicitud in itemsAgrupados)
        {
            ProductoExterno? producto = await _productosCliente.ObtenerPorId(itemSolicitud.ProductoId.Trim(), cancellationToken);

            if (producto == null)
            {
                throw new NotFoundException(
                    "ORD-004",
                    $"Producto no encontrado al crear la orden. ProductoId: {itemSolicitud.ProductoId}.");
            }

            if (producto.Stock < itemSolicitud.Cantidad)
            {
                throw new BusinessRuleException(
                    "ORD-005",
                    $"Stock insuficiente para '{producto.Nombre}'. Disponible: {producto.Stock}, solicitado: {itemSolicitud.Cantidad}.");
            }

            decimal subtotal = producto.Precio * itemSolicitud.Cantidad;
            total += subtotal;

            itemsOrden.Add(new OrdenItem
            {
                ProductoId = producto.Id,
                Cantidad = itemSolicitud.Cantidad,
                PrecioUnitario = producto.Precio
            });
        }

        var orden = new Orden
        {
            Id = Guid.NewGuid().ToString(),
            UsuarioId = solicitud.UsuarioId.Trim(),
            Estado = "Pendiente",
            Total = total,
            FechaCreacion = DateTime.UtcNow,
            FechaActualizacion = null,
            Items = itemsOrden
        };

        foreach (OrdenItem item in orden.Items)
        {
            item.OrdenId = orden.Id;
        }

        await _ordenRepositorio.Crear(orden, cancellationToken);

        _logger.LogInformation("Orden creada. OrdenId: {OrdenId}. UsuarioId: {UsuarioId}. Total: {Total}",
            orden.Id,
            orden.UsuarioId,
            orden.Total);

        return ConvertirARespuesta(orden);
    }

    public async Task<RespuestaActualizarEstadoOrden> ActualizarEstado(
        string id,
        SolicitudActualizarEstadoOrden solicitud,
        CancellationToken cancellationToken = default)
    {
        ValidarSolicitudActualizarEstado(solicitud);

        Orden orden = await ObtenerOrdenExistente(id, cancellationToken);
        string estadoNuevo = NormalizarEstado(solicitud.Estado);

        ValidarTransicionEstado(orden.Estado, estadoNuevo);

        DateTime fechaActualizacion = DateTime.UtcNow;

        await _ordenRepositorio.ActualizarEstado(id, estadoNuevo, fechaActualizacion, cancellationToken);

        _logger.LogInformation("Estado de orden actualizado. OrdenId: {OrdenId}. EstadoAnterior: {EstadoAnterior}. EstadoNuevo: {EstadoNuevo}",
            id,
            orden.Estado,
            estadoNuevo);

        return new RespuestaActualizarEstadoOrden
        {
            Id = id,
            Estado = estadoNuevo,
            FechaActualizacion = fechaActualizacion
        };
    }

    private async Task<Orden> ObtenerOrdenExistente(string id, CancellationToken cancellationToken)
    {
        Orden? orden = await _ordenRepositorio.ObtenerPorId(id, cancellationToken);

        if (orden == null)
        {
            throw new NotFoundException("ORD-001", "Orden no encontrada.");
        }

        return orden;
    }

    private static void ValidarSolicitudCrear(SolicitudCrearOrden solicitud)
    {
        var errores = new List<string>();

        if (string.IsNullOrWhiteSpace(solicitud.UsuarioId))
        {
            errores.Add("El usuarioId es obligatorio.");
        }

        if (solicitud.Items == null || solicitud.Items.Count == 0)
        {
            errores.Add("La orden debe tener al menos un item.");
        }
        else
        {
            for (int i = 0; i < solicitud.Items.Count; i++)
            {
                SolicitudCrearOrdenItem item = solicitud.Items[i];

                if (string.IsNullOrWhiteSpace(item.ProductoId))
                {
                    errores.Add($"El productoId del item {i + 1} es obligatorio.");
                }

                if (item.Cantidad <= 0)
                {
                    errores.Add($"La cantidad del item {i + 1} debe ser mayor a cero.");
                }
            }
        }

        if (errores.Count > 0)
        {
            throw new ValidationException("ORD-002", string.Join("; ", errores));
        }
    }

    private static void ValidarSolicitudActualizarEstado(SolicitudActualizarEstadoOrden solicitud)
    {
        if (string.IsNullOrWhiteSpace(solicitud.Estado))
        {
            throw new ValidationException("ORD-002", "El estado es obligatorio.");
        }

        string estadoNormalizado = NormalizarEstado(solicitud.Estado);

        if (!EstadosValidos.Contains(estadoNormalizado))
        {
            throw new ValidationException(
                "ORD-002",
                "El estado informado es invalido. Valores permitidos: Pendiente, Confirmada, Enviada, Entregada, Cancelada.");
        }
    }

    private static void ValidarTransicionEstado(string estadoActual, string estadoNuevo)
    {
        string actual = NormalizarEstado(estadoActual);
        string nuevo = NormalizarEstado(estadoNuevo);

        if (actual == nuevo)
        {
            return;
        }

        bool transicionValida = actual switch
        {
            "Pendiente" => nuevo is "Confirmada" or "Cancelada",
            "Confirmada" => nuevo is "Enviada" or "Cancelada",
            "Enviada" => nuevo is "Entregada",
            "Entregada" => false,
            "Cancelada" => false,
            _ => false
        };

        if (!transicionValida)
        {
            throw new BusinessRuleException(
                "ORD-006",
                $"Una orden en estado '{actual}' no puede pasar a '{nuevo}'.");
        }
    }

    private static string NormalizarEstado(string estado)
    {
        string valor = estado.Trim().ToLowerInvariant();

        return valor switch
        {
            "pendiente" => "Pendiente",
            "confirmada" => "Confirmada",
            "enviada" => "Enviada",
            "entregada" => "Entregada",
            "cancelada" => "Cancelada",
            _ => estado.Trim()
        };
    }

    private static List<SolicitudCrearOrdenItem> AgruparItems(IEnumerable<SolicitudCrearOrdenItem> items)
    {
        return items
            .GroupBy(i => i.ProductoId.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(g => new SolicitudCrearOrdenItem
            {
                ProductoId = g.Key,
                Cantidad = g.Sum(i => i.Cantidad)
            })
            .ToList();
    }

    private static RespuestaOrden ConvertirARespuesta(Orden orden)
    {
        return new RespuestaOrden
        {
            Id = orden.Id,
            UsuarioId = orden.UsuarioId,
            Total = orden.Total,
            Estado = orden.Estado,
            FechaCreacion = orden.FechaCreacion,
            FechaActualizacion = orden.FechaActualizacion,
            Items = orden.Items.Select(i => new RespuestaOrdenItem
            {
                ProductoId = i.ProductoId,
                Cantidad = i.Cantidad,
                PrecioUnitario = i.PrecioUnitario
            }).ToList()
        };
    }
}
