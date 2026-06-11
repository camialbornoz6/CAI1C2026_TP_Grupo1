using System.Security.Cryptography;
using System.Text;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Users.API.Data;

public class InicializadorBaseDatos
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<InicializadorBaseDatos> _logger;

    public InicializadorBaseDatos(IConfiguration configuration, ILogger<InicializadorBaseDatos> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public void Inicializar()
    {
        string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "Data Source=users.db";

        using var conexion = new SqliteConnection(connectionString);
        conexion.Open();

        conexion.Execute("""
            CREATE TABLE IF NOT EXISTS usuarios (
                id TEXT PRIMARY KEY,
                nombre TEXT NOT NULL,
                apellido TEXT NOT NULL,
                email TEXT NOT NULL UNIQUE,
                password_hash TEXT NOT NULL,
                fecha_registro TEXT NOT NULL,
                activo INTEGER NOT NULL DEFAULT 1,
                intentos_fallidos INTEGER NOT NULL DEFAULT 0,
                motivo_bloqueo TEXT NULL
            );
        """);

        int cantidad = conexion.ExecuteScalar<int>("SELECT COUNT(1) FROM usuarios;");

        if (cantidad == 0)
        {
            conexion.Execute("""
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
                VALUES
                (
                    'a1b2c3d4-0000-0000-0000-111122223333',
                    'Maria',
                    'Gonzalez',
                    'maria@email.com',
                    @PasswordMaria,
                    '2024-03-10T09:00:00Z',
                    1,
                    0,
                    NULL
                ),
                (
                    'b1b2c3d4-0000-0000-0000-111122223333',
                    'Juan',
                    'Perez',
                    'juan@email.com',
                    @PasswordJuan,
                    '2024-03-10T09:30:00Z',
                    1,
                    0,
                    NULL
                ),
                (
                    'c1b2c3d4-0000-0000-0000-111122223333',
                    'Ana',
                    'Lopez',
                    'ana.bloqueada@email.com',
                    @PasswordAna,
                    '2024-03-10T10:00:00Z',
                    0,
                    3,
                    'Fraude'
                );
            """, new
            {
                PasswordMaria = CalcularSha256("MiPassword123!"),
                PasswordJuan = CalcularSha256("Password123!"),
                PasswordAna = CalcularSha256("Password123!")
            });
        }

        _logger.LogInformation("Base de datos Users inicializada correctamente.");
    }

    private static string CalcularSha256(string texto)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(texto));
        return Convert.ToHexString(bytes);
    }
}
