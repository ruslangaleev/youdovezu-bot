using Telegram.Bot;
using Youdovezu.Application.Interfaces;
using Youdovezu.Infrastructure.Services;
using Youdovezu.Application.Models;
using Youdovezu.Infrastructure.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Явно добавляем Environment Variables Provider для корректной работы с Docker Compose
// Это обеспечивает маппинг переменных Telegram__* на Telegram:*
builder.Configuration.AddEnvironmentVariables();

// Добавляем сервисы в контейнер
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Настройка JSON сериализации для совместимости с Telegram API
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower; // Для snake_case из Telegram API
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true; // Игнорировать регистр для совместимости
        options.JsonSerializerOptions.WriteIndented = true; // Для читаемости в логах
        options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping; // Для корректной обработки Unicode (кириллица)
        
        // Добавляем кастомные конвертеры для корректной десериализации типов Telegram API
        options.JsonSerializerOptions.Converters.Add(new Youdovezu.Infrastructure.Converters.ChatTypeConverter());
        options.JsonSerializerOptions.Converters.Add(new Youdovezu.Infrastructure.Converters.UnixDateTimeConverter());
        options.JsonSerializerOptions.Converters.Add(new Youdovezu.Infrastructure.Converters.MessageEntityTypeConverter());
    });

// Настройка Swagger для документации API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Настройка CORS для работы со Swagger и внешними клиентами
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Настройка Telegram Bot сервисов с использованием IOptions<TelegramSettings>
// Configuration Mapping автоматически поддерживает переменные окружения
// Переменные TELEGRAM__BOT_TOKEN автоматически маппятся на Telegram:BotToken
builder.Services.Configure<TelegramSettings>(builder.Configuration.GetSection("Telegram"));

// Получаем настройки для валидации обязательных значений
var telegramSettings = builder.Configuration.GetSection("Telegram").Get<TelegramSettings>();

// Проверяем обязательные значения и выбрасываем исключение если они отсутствуют
if (string.IsNullOrEmpty(telegramSettings?.BotToken))
{
    throw new InvalidOperationException("Telegram:BotToken is required but not configured. " +
        "Set it in appsettings.json or via TELEGRAM__BOT_TOKEN environment variable.");
}

if (string.IsNullOrEmpty(telegramSettings?.SecretToken))
{
    throw new InvalidOperationException("Telegram:SecretToken is required but not configured. " +
        "Set it in appsettings.json or via TELEGRAM__SECRET_TOKEN environment variable.");
}

// Регистрируем Telegram Bot клиент как singleton
builder.Services.AddSingleton<ITelegramBotClient>(provider => 
    new TelegramBotClient(telegramSettings.BotToken));

// Регистрируем сервис для работы с ботом
builder.Services.AddScoped<ITelegramBotService, TelegramBotService>();

var app = builder.Build();

// Логируем источник токенов после создания приложения
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Telegram Bot Token loaded successfully from configuration");
logger.LogInformation("Telegram Secret Token loaded successfully from configuration");

// Дополнительное логирование для отладки переменных окружения
var envBotToken = Environment.GetEnvironmentVariable("Telegram__BotToken");
var envSecretToken = Environment.GetEnvironmentVariable("Telegram__SecretToken");
logger.LogInformation("Environment Telegram__BotToken: {HasValue}", !string.IsNullOrEmpty(envBotToken));
logger.LogInformation("Environment Telegram__SecretToken: {HasValue}", !string.IsNullOrEmpty(envSecretToken));

// Настройка pipeline обработки HTTP запросов
if (app.Environment.IsDevelopment())
{
    // Включаем Swagger только в режиме разработки
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Настройка CORS
app.UseCors("AllowAll");

// Добавляем middleware для валидации Telegram webhook (защита от сторонних запросов)
app.UseMiddleware<TelegramWebhookValidationMiddleware>();

//app.UseHttpsRedirection(); // Закомментировано для локальной разработки

// Маппинг контроллеров
app.MapControllers();

// Запуск приложения
app.Run();
