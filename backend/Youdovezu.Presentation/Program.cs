using Telegram.Bot;
using Youdovezu.Application.Interfaces;
using Youdovezu.Infrastructure.Services;
using Youdovezu.Application.Models;
using Youdovezu.Infrastructure.Middleware;

var builder = WebApplication.CreateBuilder(args);

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

// Настройка Telegram Bot сервисов
var telegramSettings = builder.Configuration.GetSection("TelegramBot").Get<TelegramBotSettings>();

// Поддержка переменных окружения для токенов (приоритет над appsettings)
var botToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN") ?? telegramSettings?.BotToken;
var secretToken = Environment.GetEnvironmentVariable("TELEGRAM_SECRET_TOKEN") ?? telegramSettings?.SecretToken;

if (!string.IsNullOrEmpty(botToken))
{
    // Регистрируем Telegram Bot клиент как singleton
    builder.Services.AddSingleton<ITelegramBotClient>(provider => 
        new TelegramBotClient(botToken));
    
    // Регистрируем сервис для работы с ботом
    builder.Services.AddScoped<ITelegramBotService, TelegramBotService>();
}
else
{
    // Логирование будет выполнено после создания приложения
}

var app = builder.Build();

// Логируем источник токенов после создания приложения
var logger = app.Services.GetRequiredService<ILogger<Program>>();
if (!string.IsNullOrEmpty(botToken))
{
    var tokenSource = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN") != null ? "Environment Variable" : "Configuration File";
    logger.LogInformation("Telegram Bot Token loaded from: {Source}", tokenSource);
}
else
{
    logger.LogWarning("Telegram Bot Token not found in environment variables or configuration");
}

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
