# Youdovezu Telegram Bot

Простой Telegram бот, который отвечает эхо-сообщениями (отправляет то же сообщение, которое получил от пользователя).

## Настройка

### 1. Создание бота в Telegram

1. Найдите [@BotFather](https://t.me/botfather) в Telegram
2. Отправьте команду `/newbot`
3. Следуйте инструкциям для создания бота
4. Сохраните полученный токен

### 2. Конфигурация

Обновите файлы конфигурации:

**appsettings.json** (для продакшена):
```json
{
  "TelegramBot": {
    "BotToken": "YOUR_BOT_TOKEN_HERE",
    "WebhookUrl": "https://your-domain.com/api/bot/webhook"
  }
}
```

**appsettings.Development.json** (для разработки):
```json
{
  "TelegramBot": {
    "BotToken": "YOUR_DEVELOPMENT_BOT_TOKEN_HERE",
    "WebhookUrl": "https://your-dev-domain.com/api/bot/webhook"
  }
}
```

### 3. Настройка Webhook

После развертывания приложения, установите webhook для вашего бота:

```bash
curl -X POST "https://api.telegram.org/bot<YOUR_BOT_TOKEN>/setWebhook" \
     -H "Content-Type: application/json" \
     -d '{"url": "https://your-domain.com/api/bot/webhook"}'
```

## Запуск

### Локальная разработка

```bash
cd backend
dotnet run --project Youdovezu.Presentation
```

### Docker

```bash
cd backend
docker-compose up --build
```

## API Endpoints

- `POST /api/bot/webhook` - Webhook для получения обновлений от Telegram
- `GET /api/bot/health` - Проверка состояния бота

## Функциональность

Бот реализует простую эхо-функциональность:
- Получает текстовое сообщение от пользователя
- Отправляет то же сообщение обратно пользователю
- Логирует все операции

## Структура проекта

- `Youdovezu.Application` - Бизнес-логика и сервисы
- `Youdovezu.Domain` - Доменные модели
- `Youdovezu.Infrastructure` - Инфраструктурные компоненты
- `Youdovezu.Presentation` - Web API и контроллеры
