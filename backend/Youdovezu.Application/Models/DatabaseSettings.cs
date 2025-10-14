namespace Youdovezu.Application.Models;

/// <summary>
/// Настройки подключения к базе данных
/// </summary>
public class DatabaseSettings
{
    /// <summary>
    /// Строка подключения к базе данных PostgreSQL
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Хост базы данных
    /// </summary>
    public string Host { get; set; } = "localhost";

    /// <summary>
    /// Порт базы данных
    /// </summary>
    public int Port { get; set; } = 5432;

    /// <summary>
    /// Имя базы данных
    /// </summary>
    public string Database { get; set; } = "youdovezu";

    /// <summary>
    /// Имя пользователя для подключения к базе данных
    /// </summary>
    public string Username { get; set; } = "youdovezu";

    /// <summary>
    /// Пароль для подключения к базе данных
    /// </summary>
    public string Password { get; set; } = string.Empty;
}
