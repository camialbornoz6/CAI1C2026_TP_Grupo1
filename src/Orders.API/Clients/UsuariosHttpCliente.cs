using System.Net;

namespace Orders.API.Clients;

public class UsuariosHttpCliente : IUsuariosCliente
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<UsuariosHttpCliente> _logger;

    public UsuariosHttpCliente(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<UsuariosHttpCliente> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> ExisteUsuario(string usuarioId, CancellationToken cancellationToken = default)
    {
        bool validacionHabilitada = _configuration.GetValue<bool>("ExternalServices:UsersApi:ValidationEnabled", false);

        if (!validacionHabilitada)
        {
            _logger.LogWarning("Validacion HTTP contra Users.API deshabilitada por configuracion. UsuarioId aceptado temporalmente: {UsuarioId}", usuarioId);
            return true;
        }

        HttpClient cliente = _httpClientFactory.CreateClient("Users.API");

        using HttpResponseMessage respuesta = await cliente.GetAsync($"api/users/{usuarioId}", cancellationToken);

        if (respuesta.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        respuesta.EnsureSuccessStatusCode();
        return true;
    }
}
