using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Products.API.HealthChecks;

public class ApiStatusHealthCheck : IHealthCheck
{
    private static readonly DateTime FechaInicio = DateTime.UtcNow;

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var datos = new Dictionary<string, object>
        {
            ["service"] = "Products.API",
            ["startedAtUtc"] = FechaInicio,
            ["uptimeSeconds"] = Convert.ToInt64((DateTime.UtcNow - FechaInicio).TotalSeconds),
            ["dotnetVersion"] = Environment.Version.ToString()
        };

        return Task.FromResult(HealthCheckResult.Healthy("API operativa.", datos));
    }
}
