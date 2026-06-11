using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Orders.API.HealthChecks;

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
            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "Data Source=orders.db";

            using var conexion = new SqliteConnection(connectionString);
            await conexion.OpenAsync(cancellationToken);
            await conexion.ExecuteScalarAsync<int>("SELECT 1;");

            return HealthCheckResult.Healthy("SQLite responde correctamente.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("No se pudo conectar a SQLite.", ex);
        }
    }
}
