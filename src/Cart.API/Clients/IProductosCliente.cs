namespace Cart.API.Clients;

public interface IProductosCliente
{
    Task<ProductoExterno?> ObtenerPorId(string productoId, CancellationToken cancellationToken = default);
}
