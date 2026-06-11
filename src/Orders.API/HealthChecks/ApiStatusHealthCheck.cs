using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Orders.API.HealthChecks;

public class ApiStatusHealthCheck : IHealthCheck
{
    private readonly DateTime _fechaInicioUtc;

    public ApiStatusHealthCheck()
    {
        _fechaInicioUtc = DateTime.UtcNow;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>
        {
            ["uptimeSeconds"] = (DateTime.UtcNow - _fechaInicioUtc).TotalSeconds,
            ["dotnetVersion"] = Environment.Version.ToString(),
            ["startedAtUtc"] = _fechaInicioUtc.ToString("O")
        };

        return Task.FromResult(HealthCheckResult.Healthy("Orders.API operativo.", data));
    }
}
