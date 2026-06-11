using System.Net;
using System.Text.Json;

namespace Notifications.API.Clients;

public class UsuariosHttpCliente : IUsuariosCliente
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<UsuariosHttpCliente> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public UsuariosHttpCliente(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor,
        ILogger<UsuariosHttpCliente> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<bool> ExisteUsuario(string usuarioId, CancellationToken cancellationToken = default)
    {
        bool validacionHabilitada = _configuration.GetValue<bool>("ExternalServices:UsersApi:ValidationEnabled", true);

        if (!validacionHabilitada)
        {
            _logger.LogWarning("Validacion HTTP contra Users.API deshabilitada por configuracion.");
            return true;
        }

        HttpClient cliente = _httpClientFactory.CreateClient("Users.API");

        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/users/{usuarioId}");
        AgregarCorrelationId(request);

        using HttpResponseMessage respuesta = await cliente.SendAsync(request, cancellationToken);

        if (respuesta.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        respuesta.EnsureSuccessStatusCode();

        string contenido = await respuesta.Content.ReadAsStringAsync(cancellationToken);
        UsuarioExterno? usuario = JsonSerializer.Deserialize<UsuarioExterno>(contenido, JsonOptions);

        return usuario != null && !string.IsNullOrWhiteSpace(usuario.Id);
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
