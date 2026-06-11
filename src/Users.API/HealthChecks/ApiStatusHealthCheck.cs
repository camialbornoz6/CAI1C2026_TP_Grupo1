using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Users.API.HealthChecks;

public class ApiStatusHealthCheck : IHealthCheck
{
    private static readonly DateTime FechaInicioUtc = DateTime.UtcNow;

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>
        {
            ["service"] = "Users.API",
            ["startedAtUtc"] = FechaInicioUtc,
            ["uptimeSeconds"] = (DateTime.UtcNow - FechaInicioUtc).TotalSeconds,
            ["dotnetVersion"] = Environment.Version.ToString()
        };

        return Task.FromResult(HealthCheckResult.Healthy("La API esta operativa.", data));
    }
}
