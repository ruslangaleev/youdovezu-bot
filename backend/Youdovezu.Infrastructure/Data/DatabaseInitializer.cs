using Npgsql;

namespace Youdovezu.Infrastructure.Data;

/// <summary>
/// Создание базы данных PostgreSQL при отсутствии (перед применением миграций).
/// </summary>
public static class DatabaseInitializer
{
    private const string AdminDatabase = "postgres";

    /// <summary>
    /// Подключается к административной БД (postgres) и создаёт целевую БД, если её нет.
    /// </summary>
    public static async Task EnsurePostgresDatabaseExistsAsync(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return;

        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        var databaseName = builder.Database;
        if (string.IsNullOrWhiteSpace(databaseName))
            return;

        builder.Database = AdminDatabase;
        var adminConnectionString = builder.ToString();

        await using var connection = new NpgsqlConnection(adminConnectionString);
        await connection.OpenAsync();

        await using var checkCmd = new NpgsqlCommand(
            "SELECT 1 FROM pg_database WHERE datname = @name", connection);
        checkCmd.Parameters.AddWithValue("name", databaseName);
        var exists = await checkCmd.ExecuteScalarAsync() != null;

        if (exists)
            return;

        await using var createCmd = new NpgsqlCommand(
            $"CREATE DATABASE \"{databaseName.Replace("\"", "\"\"")}\"", connection);
        await createCmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Создаёт таблицу __EFMigrationsHistory в целевой БД, если её ещё нет (нужно для пустой БД до первого MigrateAsync).
    /// </summary>
    public static async Task EnsureMigrationsHistoryTableExistsAsync(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return;

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            """
            CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
                "MigrationId" character varying(150) NOT NULL PRIMARY KEY,
                "ProductVersion" character varying(32) NOT NULL
            );
            """,
            connection);
        await cmd.ExecuteNonQueryAsync();
    }
}
