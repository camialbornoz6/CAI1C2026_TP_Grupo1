using Products.API.DTOs;

namespace Products.API.Services;

public interface IProductoServicio
{
    IEnumerable<RespuestaProducto> ObtenerTodos(string? categoria, string? nombre);

    RespuestaProducto ObtenerPorId(string id);

    RespuestaProducto Crear(SolicitudCrearProducto solicitud);

    RespuestaProducto Actualizar(string id, SolicitudActualizarProducto solicitud);

    void Eliminar(string id);
}
