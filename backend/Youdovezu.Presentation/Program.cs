using Telegram.Bot;
using Youdovezu.Application.Interfaces;
using Youdovezu.Infrastructure.Services;
using Youdovezu.Application.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower; // Для snake_case
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true; // Игнорировать регистр
        options.JsonSerializerOptions.WriteIndented = true; // Для читаемости в логах
        options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping; // Для корректной обработки Unicode
        options.JsonSerializerOptions.Converters.Add(new Youdovezu.Infrastructure.Converters.ChatTypeConverter());
        options.JsonSerializerOptions.Converters.Add(new Youdovezu.Infrastructure.Converters.UnixDateTimeConverter());
        options.JsonSerializerOptions.Converters.Add(new Youdovezu.Infrastructure.Converters.MessageEntityTypeConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure Telegram Bot
var telegramSettings = builder.Configuration.GetSection("TelegramBot").Get<TelegramBotSettings>();
if (telegramSettings?.BotToken != null)
{
    builder.Services.AddSingleton<ITelegramBotClient>(provider => 
        new TelegramBotClient(telegramSettings.BotToken));
    builder.Services.AddScoped<ITelegramBotService, TelegramBotService>();
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

//app.UseHttpsRedirection();

app.MapControllers();

app.Run();
