using System.Net;
using System.Text.Json;

namespace Cart.API.Clients;

public class ProductosHttpCliente : IProductosCliente
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ProductosHttpCliente> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ProductosHttpCliente(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor,
        ILogger<ProductosHttpCliente> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
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

        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/products/{productoId}");
        AgregarCorrelationId(request);

        using HttpResponseMessage respuesta = await cliente.SendAsync(request, cancellationToken);

        if (respuesta.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        respuesta.EnsureSuccessStatusCode();

        string contenido = await respuesta.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<ProductoExterno>(contenido, JsonOptions);
    }

    private void AgregarCorrelationId(HttpRequestMessage request)
    {
        string? correlationId = _httpContextAccessor.HttpContext?.Response.Headers["X-Correlation-Id"].ToString();

        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = _httpContextAccessor.HttpContext?.Request.Headers["X-Correlation-Id"].ToString();
        }

        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            request.Headers.TryAddWithoutValidation("X-Correlation-Id", correlationId);
        }
    }
}
