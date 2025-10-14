using Telegram.Bot;
using Youdovezu.Application.Interfaces;
using Youdovezu.Application.Services;
using Youdovezu.Application.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
