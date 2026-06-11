using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Notifications.API.HealthChecks;

public class ApiStatusHealthCheck : IHealthCheck
{
    private static readonly DateTime FechaInicio = DateTime.UtcNow;

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>
        {
            ["uptimeSeconds"] = (DateTime.UtcNow - FechaInicio).TotalSeconds,
            ["dotnetVersion"] = Environment.Version.ToString(),
            ["startedAtUtc"] = FechaInicio.ToString("O")
        };

        return Task.FromResult(HealthCheckResult.Healthy("Notifications.API operativo.", data));
    }
}
