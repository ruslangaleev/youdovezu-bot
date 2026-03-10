using Telegram.Bot;
using Youdovezu.Application.Interfaces;
using Youdovezu.Infrastructure.Services;
using Youdovezu.Application.Models;
using Youdovezu.Infrastructure.Middleware;
using Youdovezu.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Youdovezu.Domain.Interfaces;
using Youdovezu.Infrastructure.Repositories;
using Youdovezu.Application.Services;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Настройка лимитов Kestrel для загрузки больших файлов
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 50 * 1024 * 1024; // 50 MB
    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
    options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(1);
});

// Явно добавляем Environment Variables Provider для корректной работы с Docker Compose
// Это обеспечивает маппинг переменных Telegram__* на Telegram:*
builder.Configuration.AddEnvironmentVariables();

// Добавляем сервисы в контейнер
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Настройка JSON сериализации для совместимости с фронтендом
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase; // Для camelCase совместимости с фронтендом
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
    options.AddPolicy("AllowTunaClient", policy =>
    {
        policy.WithOrigins(
                "https://client-youdovezu.ru.tuna.am",  // Продакшн origin
                "http://localhost:3000"  // Локальная разработка
              )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .WithExposedHeaders("Content-Disposition")
              .AllowCredentials();  // Если используешь initData, куки и т.п.
    });
});

// Настройка лимитов для загрузки файлов
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 50 * 1024 * 1024; // 50 MB
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartHeadersLengthLimit = int.MaxValue;
});

// Настройка Entity Framework с PostgreSQL
builder.Services.AddDbContext<YoudovezuDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? builder.Configuration["Database:ConnectionString"];
    
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("Database connection string is not configured. " +
            "Set Database:ConnectionString in appsettings.json or DATABASE__CONNECTIONSTRING environment variable.");
    }
    
    options.UseNpgsql(connectionString);
});

// Настройка Telegram Bot сервисов с использованием IOptions<TelegramSettings>
// Configuration Mapping автоматически поддерживает переменные окружения
// Переменные TELEGRAM__BOT_TOKEN автоматически маппятся на Telegram:BotToken
builder.Services.Configure<TelegramSettings>(builder.Configuration.GetSection("Telegram"));

// Настройка Database Settings
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("Database"));

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

// Регистрируем репозитории и сервисы
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITripRepository, TripRepository>();
builder.Services.AddScoped<ITripService, TripService>();
builder.Services.AddScoped<Youdovezu.Domain.Interfaces.IDriverDocumentsRepository, DriverDocumentsRepository>();
builder.Services.AddScoped<IDriverDocumentsService, DriverDocumentsService>();
builder.Services.AddScoped<ISettlementRepository, SettlementRepository>();
builder.Services.AddScoped<ITripOfferRepository, TripOfferRepository>();
builder.Services.AddScoped<ITripSearchService, TripSearchService>();
builder.Services.AddScoped<ITripOfferService, TripOfferService>();

// Регистрируем сервис валидации Telegram WebApp
builder.Services.AddScoped<TelegramWebAppValidationService>(provider =>
{
    var botToken = builder.Configuration["Telegram:BotToken"] 
        ?? throw new InvalidOperationException("Telegram BotToken is not configured");
    var logger = provider.GetRequiredService<ILogger<TelegramWebAppValidationService>>();
    return new TelegramWebAppValidationService(botToken, logger);
});

var app = builder.Build();

// Настройка CORS (должен быть ПЕРВЫМ, до других middleware)
// Включаем обработку preflight запросов
app.UseCors("AllowTunaClient");

// Логируем источник токенов после создания приложения
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Telegram Bot configuration loaded successfully");

// Автоматическое применение миграций при запуске приложения
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<YoudovezuDbContext>();
    try
    {
        // Применяем все ожидающие миграции
        await context.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while applying database migrations");
        throw;
    }
}


// Дополнительное логирование для отладки переменных окружения (только в Development)
if (app.Environment.IsDevelopment())
{
    var envBotToken = Environment.GetEnvironmentVariable("Telegram__BotToken");
    var envSecretToken = Environment.GetEnvironmentVariable("Telegram__SecretToken");
    logger.LogInformation("Environment Telegram__BotToken: {HasValue}", !string.IsNullOrEmpty(envBotToken));
    logger.LogInformation("Environment Telegram__SecretToken: {HasValue}", !string.IsNullOrEmpty(envSecretToken));
}

// Настройка pipeline обработки HTTP запросов
if (app.Environment.IsDevelopment())
{
    // Включаем Swagger только в режиме разработки
    app.UseSwagger();
    app.UseSwaggerUI();
}



// Логирование для отладки CORS
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api/webapp/driver-documents/upload"))
    {
        logger.LogInformation("Upload endpoint request: Method={Method}, Origin={Origin}, ContentType={ContentType}, ContentLength={ContentLength}",
            context.Request.Method,
            context.Request.Headers["Origin"].ToString(),
            context.Request.ContentType,
            context.Request.ContentLength);
    }
    await next();
    
    // Логируем CORS заголовки в ответе
    if (context.Request.Path.StartsWithSegments("/api/webapp/driver-documents/upload"))
    {
        logger.LogInformation("Upload endpoint response: StatusCode={StatusCode}, CORS-Origin={CorsOrigin}",
            context.Response.StatusCode,
            context.Response.Headers["Access-Control-Allow-Origin"].ToString());
    }
});


// Настройка статических файлов для доступа к загруженным изображениям
// Сначала настраиваем обслуживание файлов из папки uploads
var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "uploads");
try
{
    if (!Directory.Exists(uploadsPath))
    {
        Directory.CreateDirectory(uploadsPath);
        logger.LogInformation("Created uploads directory at: {UploadsPath}", uploadsPath);
    }
    else
    {
        logger.LogInformation("Uploads directory exists at: {UploadsPath}", uploadsPath);
        // Логируем количество файлов в uploads для отладки
        try
        {
            var filesCount = Directory.GetFiles(uploadsPath, "*", SearchOption.AllDirectories).Length;
            logger.LogInformation("Found {FilesCount} files in uploads directory", filesCount);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not count files in uploads directory");
        }
    }
    
    // Проверяем права доступа к директории
    var directoryInfo = new DirectoryInfo(uploadsPath);
    logger.LogInformation("Uploads directory permissions: CanWrite={CanWrite}, Exists={Exists}", 
        directoryInfo.Exists, 
        directoryInfo.Exists && (File.GetAttributes(uploadsPath) & FileAttributes.ReadOnly) == 0);
}
catch (Exception ex)
{
    logger.LogError(ex, "Error setting up uploads directory at: {UploadsPath}", uploadsPath);
    throw;
}

// Функция для определения Content-Type
static string GetContentType(string extension) => extension switch
{
    ".jpg" => "image/jpeg",
    ".jpeg" => "image/jpeg",
    ".png" => "image/png",
    ".gif" => "image/gif",
    ".webp" => "image/webp",
    _ => "application/octet-stream"
};

// Настраиваем статические файлы из папки uploads с правильным RequestPath
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads",
    ServeUnknownFileTypes = true, // Разрешаем обслуживание любых типов файлов (для изображений)
    OnPrepareResponse = ctx =>
    {
        // Устанавливаем правильные заголовки для изображений
        var fileExtension = Path.GetExtension(ctx.File.Name).ToLowerInvariant();
        if (fileExtension == ".jpg" || fileExtension == ".jpeg" || fileExtension == ".png" || 
            fileExtension == ".gif" || fileExtension == ".webp")
        {
            ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=3600");
            ctx.Context.Response.Headers.Append("Content-Type", GetContentType(fileExtension));
        }
    }
});

// Также настраиваем стандартные статические файлы из wwwroot (если нужны)
app.UseStaticFiles();

// Добавляем middleware для валидации Telegram webhook (защита от сторонних запросов)
// НО исключаем статические файлы и endpoint загрузки файлов из валидации
//app.UseWhen(context => 
//    !context.Request.Path.StartsWithSegments("/uploads") &&
//    !context.Request.Path.StartsWithSegments("/api/webapp/driver-documents/upload"),
//    appBuilder =>
//{
//    appBuilder.UseMiddleware<TelegramWebhookValidationMiddleware>();
//});

// HTTPS редирект отключен для локальной разработки
// app.UseHttpsRedirection();

// Маппинг контроллеров
app.MapControllers();

// Запуск приложения
app.Run();
