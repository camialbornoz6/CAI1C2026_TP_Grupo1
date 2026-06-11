using System.Net;
using System.Text.Json;

namespace Orders.API.Clients;

public class ProductosHttpCliente : IProductosCliente
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ProductosHttpCliente> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ProductosHttpCliente(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<ProductosHttpCliente> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ProductoExterno?> ObtenerPorId(string productoId, CancellationToken cancellationToken = default)
    {
        bool validacionHabilitada = _configuration.GetValue<bool>("ExternalServices:ProductsApi:ValidationEnabled", true);

        if (!validacionHabilitada)
        {
            _logger.LogWarning("Validacion HTTP contra Products.API deshabilitada por configuracion.");
            return new ProductoExterno
            {
                Id = productoId,
                Nombre = "Producto sin validacion externa",
                Precio = 1,
                Stock = int.MaxValue,
                Categoria = "Sin validar",
                FechaCreacion = DateTime.UtcNow
            };
        }

        HttpClient cliente = _httpClientFactory.CreateClient("Products.API");

        using HttpResponseMessage respuesta = await cliente.GetAsync($"api/products/{productoId}", cancellationToken);

        if (respuesta.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        respuesta.EnsureSuccessStatusCode();

        string contenido = await respuesta.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<ProductoExterno>(contenido, JsonOptions);
    }
}
