using Microsoft.EntityFrameworkCore;
using YouDovezu.Infrastructure.Data;
using YouDovezu.Application.Handlers.Health;
using YouDovezu.Application.Queries.Health;
using YouDovezu.Application.Common.Interfaces;
using YouDovezu.Application.Services;
using YouDovezu.Infrastructure.Services;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// КОНФИГУРАЦИЯ СЕРВИСОВ
// ============================================================================

// Основные сервисы ASP.NET Core
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// База данных PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=localhost;Database=youdovezu;Username=postgres;Password=postgres";

builder.Services.AddDbContext<YouDovezuDbContext>(options =>
    options.UseNpgsql(connectionString));

// Регистрация интерфейса контекста базы данных
builder.Services.AddScoped<IYouDovezuDbContext>(provider => 
    provider.GetRequiredService<YouDovezuDbContext>());

// MediatR для CQRS паттерна
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(GetHealthStatusQueryHandler).Assembly);
});

// Telegram Bot API клиент (TODO: Реализовать интеграцию)
// var telegramBotToken = builder.Configuration["TelegramBot:Token"] ?? "YOUR_BOT_TOKEN";
// builder.Services.AddSingleton<ITelegramBotClient>(provider => new TelegramBotClient(telegramBotToken));

// ============================================================================
// РЕГИСТРАЦИЯ СЕРВИСОВ ПО СЛОЯМ АРХИТЕКТУРЫ
// ============================================================================

// Application Services - бизнес-логика
builder.Services.AddScoped<IMessageTemplateService, MessageTemplateService>();

// Infrastructure Services - внешние сервисы и инструменты
builder.Services.AddScoped<ITelegramService, TelegramService>();

var app = builder.Build();

// ============================================================================
// КОНФИГУРАЦИЯ HTTP PIPELINE
// ============================================================================

// Swagger UI для разработки
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware для обработки HTTP запросов
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// ============================================================================
// ИНИЦИАЛИЗАЦИЯ БАЗЫ ДАННЫХ
// ============================================================================

// Создание базы данных при первом запуске
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<YouDovezuDbContext>();
    context.Database.EnsureCreated();
}

// Запуск приложения
app.Run();