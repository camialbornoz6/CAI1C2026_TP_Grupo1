using Dapper;
using Microsoft.Data.Sqlite;
using Users.API.Models;

namespace Users.API.Repositories;

public class UsuarioRepositorio : IUsuarioRepositorio
{
    private readonly IConfiguration _configuration;

    public UsuarioRepositorio(IConfiguration configuration)
    {
        _configuration = configuration;
    }


    public async Task<Usuario?> ObtenerPorId(string id, CancellationToken cancellationToken = default)
    {
        using var conexion = CrearConexion();
        await conexion.OpenAsync(cancellationToken);

        string consulta = """
            SELECT
                id AS Id,
                nombre AS Nombre,
                apellido AS Apellido,
                email AS Email,
                password_hash AS PasswordHash,
                fecha_registro AS FechaRegistro,
                activo AS Activo,
                intentos_fallidos AS IntentosFallidos,
                motivo_bloqueo AS MotivoBloqueo
            FROM usuarios
            WHERE id = @Id;
        """;

        return await conexion.QuerySingleOrDefaultAsync<Usuario>(consulta, new
        {
            Id = id.Trim()
        });
    }

    public async Task<Usuario?> ObtenerPorEmail(string email, CancellationToken cancellationToken = default)
    {
        using var conexion = CrearConexion();
        await conexion.OpenAsync(cancellationToken);

        string consulta = """
            SELECT
                id AS Id,
                nombre AS Nombre,
                apellido AS Apellido,
                email AS Email,
                password_hash AS PasswordHash,
                fecha_registro AS FechaRegistro,
                activo AS Activo,
                intentos_fallidos AS IntentosFallidos,
                motivo_bloqueo AS MotivoBloqueo
            FROM usuarios
            WHERE lower(email) = lower(@Email);
        """;

        return await conexion.QuerySingleOrDefaultAsync<Usuario>(consulta, new
        {
            Email = email.Trim()
        });
    }

    public async Task Crear(Usuario usuario, CancellationToken cancellationToken = default)
    {
        using var conexion = CrearConexion();
        await conexion.OpenAsync(cancellationToken);

        await conexion.ExecuteAsync("""
            INSERT INTO usuarios (
                id,
                nombre,
                apellido,
                email,
                password_hash,
                fecha_registro,
                activo,
                intentos_fallidos,
                motivo_bloqueo
            )
            VALUES (
                @Id,
                @Nombre,
                @Apellido,
                @Email,
                @PasswordHash,
                @FechaRegistro,
                @Activo,
                @IntentosFallidos,
                @MotivoBloqueo
            );
        """, new
        {
            usuario.Id,
            usuario.Nombre,
            usuario.Apellido,
            usuario.Email,
            usuario.PasswordHash,
            FechaRegistro = usuario.FechaRegistro.ToString("O"),
            Activo = usuario.Activo ? 1 : 0,
            usuario.IntentosFallidos,
            usuario.MotivoBloqueo
        });
    }

    public async Task<int> IncrementarIntentoFallido(string email, CancellationToken cancellationToken = default)
    {
        using var conexion = CrearConexion();
        await conexion.OpenAsync(cancellationToken);

        await conexion.ExecuteAsync("""
            UPDATE usuarios
            SET intentos_fallidos = intentos_fallidos + 1
            WHERE lower(email) = lower(@Email);
        """, new { Email = email.Trim() });

        return await conexion.ExecuteScalarAsync<int>("""
            SELECT intentos_fallidos
            FROM usuarios
            WHERE lower(email) = lower(@Email);
        """, new { Email = email.Trim() });
    }

    public async Task BloquearPorIntentosFallidos(string email, CancellationToken cancellationToken = default)
    {
        using var conexion = CrearConexion();
        await conexion.OpenAsync(cancellationToken);

        await conexion.ExecuteAsync("""
            UPDATE usuarios
            SET activo = 0,
                motivo_bloqueo = 'IntentosFallidos'
            WHERE lower(email) = lower(@Email);
        """, new { Email = email.Trim() });
    }

    public async Task ReiniciarIntentosFallidos(string email, CancellationToken cancellationToken = default)
    {
        using var conexion = CrearConexion();
        await conexion.OpenAsync(cancellationToken);

        await conexion.ExecuteAsync("""
            UPDATE usuarios
            SET intentos_fallidos = 0
            WHERE lower(email) = lower(@Email);
        """, new { Email = email.Trim() });
    }

    private SqliteConnection CrearConexion()
    {
        string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "Data Source=users.db";
        return new SqliteConnection(connectionString);
    }
}
