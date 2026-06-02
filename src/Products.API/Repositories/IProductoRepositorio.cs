using Products.API.Models;

namespace Products.API.Repositories;

public interface IProductoRepositorio
{
    IEnumerable<Producto> ObtenerTodos(string? categoria, string? nombre);

    Producto? ObtenerPorId(string id);

    bool ExisteProductoConNombreYCategoria(string nombre, string categoria);

    bool ExisteProductoConNombreYCategoriaExcluyendoId(string id, string nombre, string categoria);

    void Crear(Producto producto);

    void Actualizar(Producto producto);

    void Eliminar(string id);

    bool TieneOrdenesActivas(string id);
}
