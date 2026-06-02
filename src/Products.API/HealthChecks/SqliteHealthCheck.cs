using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Products.API.HealthChecks;

public class SqliteHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuracion;

    public SqliteHealthCheck(IConfiguration configuracion)
    {
        _configuracion = configuracion;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            string cadenaConexion = _configuracion.GetConnectionString("DefaultConnection")
                ?? "Data Source=products.db";

            using var conexion = new SqliteConnection(cadenaConexion);
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
