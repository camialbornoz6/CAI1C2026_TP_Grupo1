using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Cart.API.HealthChecks;

public class SqliteHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;

    public SqliteHealthCheck(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "Data Source=cart.db";

            using var conexion = new SqliteConnection(connectionString);
            await conexion.OpenAsync(cancellationToken);

            int resultado = await conexion.ExecuteScalarAsync<int>("SELECT 1;");

            var data = new Dictionary<string, object>
            {
                ["database"] = "cart.db",
                ["select1"] = resultado
            };

            return HealthCheckResult.Healthy("Conexion SQLite OK.", data);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("No se pudo conectar a SQLite.", ex);
        }
    }
}
