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

#### Способ 1: Через curl (рекомендуется)

```bash
curl -X POST "https://api.telegram.org/bot<YOUR_BOT_TOKEN>/setWebhook" \
     -H "Content-Type: application/json" \
     -d '{"url": "https://your-domain.com/api/bot/webhook"}'
```

**Пример с реальным токеном:**
```bash
curl -X POST "https://api.telegram.org/bot7642287932:AAGAaP0BdJgvxrE3UEfdAoDcrJ0D9TzmSJI/setWebhook" \
     -H "Content-Type: application/json" \
     -d '{"url": "https://your-domain.com/api/bot/webhook"}'
```

#### Способ 2: Через браузер

Откройте в браузере:
```
https://api.telegram.org/bot<YOUR_BOT_TOKEN>/setWebhook?url=https://your-domain.com/api/bot/webhook
```

**Пример с реальным токеном:**
```
https://api.telegram.org/bot7642287932:AAGAaP0BdJgvxrE3UEfdAoDcrJ0D9TzmSJI/setWebhook?url=https://your-domain.com/api/bot/webhook
```

#### Проверка webhook

Проверить текущий webhook:
```bash
curl "https://api.telegram.org/bot<YOUR_BOT_TOKEN>/getWebhookInfo"
```

Удалить webhook (если нужно):
```bash
curl -X POST "https://api.telegram.org/bot<YOUR_BOT_TOKEN>/deleteWebhook"
```

#### Важные замечания:

- **HTTPS обязателен**: Telegram принимает только HTTPS webhook URL
- **Публичный доступ**: Ваш сервер должен быть доступен из интернета
- **SSL сертификат**: Должен быть валидный SSL сертификат
- **Порт**: Убедитесь, что порт открыт и доступен

## Запуск

### Локальная разработка

```bash
cd backend
dotnet run --project Youdovezu.Presentation
```

**Порты приложения:**
- **HTTP**: `http://localhost:8080`
- **HTTPS**: `https://localhost:7007`
- **Swagger UI**: `https://localhost:7007/swagger` (в режиме Development)

### Docker

```bash
cd backend
docker-compose up --build
```

**Docker порты:**
- **HTTP**: `http://localhost:8080`
- **HTTPS**: `http://localhost:8081`
- **Swagger UI**: `http://localhost:8081/swagger` (в режиме Development)

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
