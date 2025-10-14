# Youdovezu Telegram Bot

Простой Telegram бот, который отвечает эхо-сообщениями (отправляет то же сообщение, которое получил от пользователя).

## Настройка

### 1. Создание бота в Telegram

1. Найдите [@BotFather](https://t.me/botfather) в Telegram
2. Отправьте команду `/newbot`
3. Следуйте инструкциям для создания бота
4. Сохраните полученный токен

### 2. Конфигурация

Обновите файлы конфигурации или используйте переменные окружения:

**appsettings.json** (для продакшена):
```json
{
  "TelegramBot": {
    "BotToken": "YOUR_BOT_TOKEN_HERE",
    "WebhookUrl": "https://your-domain.com/api/bot/webhook",
    "SecretToken": "your-secret-token-here"
  }
}
```

**appsettings.Development.json** (для разработки):
```json
{
  "TelegramBot": {
    "BotToken": "YOUR_DEVELOPMENT_BOT_TOKEN_HERE",
    "WebhookUrl": "https://your-dev-domain.com/api/bot/webhook",
    "SecretToken": "dev-secret-token-123"
  }
}
```

#### Переменные окружения (рекомендуется для продакшена)

Приложение поддерживает переменные окружения с приоритетом над файлами конфигурации:

**Windows:**
```cmd
set TELEGRAM_BOT_TOKEN=7642287932:AAGAaP0BdJgvxrE3UEfdAoDcrJ0D9TzmSJI
set TELEGRAM_SECRET_TOKEN=your-secret-token-here
```

**Linux/Mac:**
```bash
export TELEGRAM_BOT_TOKEN=7642287932:AAGAaP0BdJgvxrE3UEfdAoDcrJ0D9TzmSJI
export TELEGRAM_SECRET_TOKEN=your-secret-token-here
```

**Docker:**
```bash
docker run -e TELEGRAM_BOT_TOKEN=7642287932:AAGAaP0BdJgvxrE3UEfdAoDcrJ0D9TzmSJI \
           -e TELEGRAM_SECRET_TOKEN=your-secret-token-here \
           your-app
```

**Docker Compose:**
```yaml
services:
  youdovezu.presentation:
    environment:
      - TELEGRAM_BOT_TOKEN=7642287932:AAGAaP0BdJgvxrE3UEfdAoDcrJ0D9TzmSJI
      - TELEGRAM_SECRET_TOKEN=your-secret-token-here
```

### 3. Настройка Webhook с секретным токеном

После развертывания приложения, установите webhook для вашего бота с секретным токеном:

#### Способ 1: Через curl (рекомендуется)

```bash
curl -X POST "https://api.telegram.org/bot<YOUR_BOT_TOKEN>/setWebhook" \
     -H "Content-Type: application/json" \
     -d '{"url": "https://your-domain.com/api/bot/webhook", "secret_token": "your-secret-token-here"}'
```

**Пример с реальным токеном:**
```bash
curl --location 'https://api.telegram.org/bot7642287932:AAGAaP0BdJgvxrE3UEfdAoDcrJ0D9TzmSJI/setWebhook' \
--header 'Content-Type: application/json' \
--data '{
    "url": "https://youdovezu.ru.tuna.am/api/bot/webhook",
    "secret_token": "dev-secret-token-123"
}'
```

**Для чего нужен secret_token:**
- **Защита от сторонних запросов**: Telegram отправляет этот токен в заголовке `X-Telegram-Bot-Api-Secret-Token` с каждым webhook запросом
- **Валидация подлинности**: Ваш сервер проверяет соответствие полученного токена с ожидаемым
- **Блокировка атак**: Подозрительные запросы без правильного токена блокируются с HTTP 401
- **Предотвращение спама**: Защищает от попыток взлома и DoS атак на ваш бот

#### Способ 2: Через браузер

Откройте в браузере:
```
https://api.telegram.org/bot<YOUR_BOT_TOKEN>/setWebhook?url=https://your-domain.com/api/bot/webhook&secret_token=your-secret-token-here
```

**Пример с реальным токеном:**
```
https://api.telegram.org/bot7642287932:AAGAaP0BdJgvxrE3UEfdAoDcrJ0D9TzmSJI/setWebhook?url=https://your-domain.com/api/bot/webhook&secret_token=your-secret-token-here
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
- **Секретный токен**: Обязательно используйте секретный токен для защиты webhook

## Безопасность

### Защита Webhook

Приложение защищено от сторонних запросов с помощью middleware валидации:

- **Секретный токен**: Telegram отправляет токен в заголовке `X-Telegram-Bot-Api-Secret-Token`
- **Валидация**: Middleware проверяет соответствие токена перед обработкой запроса
- **Логирование**: Все попытки несанкционированного доступа логируются
- **Блокировка**: Невалидные запросы возвращают HTTP 401 Unauthorized

### Настройка секретного токена

1. **Сгенерируйте случайный токен** (минимум 16 символов)
2. **Добавьте в конфигурацию** `SecretToken`
3. **Установите webhook с токеном** через Telegram API
4. **Telegram будет отправлять токен** в каждом запросе

**Пример генерации токена:**
```bash
# Linux/Mac
openssl rand -hex 16

# Windows PowerShell
[System.Web.Security.Membership]::GeneratePassword(32, 0)
```

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

#### Локальная разработка

```bash
cd backend
docker-compose up --build
```

**Docker порты:**
- **HTTP**: `http://localhost:8080`
- **HTTPS**: `http://localhost:8081`
- **Swagger UI**: `http://localhost:8081/swagger` (в режиме Development)

#### Продакшен

1. **Создайте файл `.env`** в папке backend:
```bash
# Скопируйте env.example в .env и заполните реальными значениями
cp env.example .env
```

2. **Заполните переменные в `.env`:**
```bash
TELEGRAM_BOT_TOKEN=your-real-bot-token
TELEGRAM_SECRET_TOKEN=your-secret-token
```

3. **Запустите в продакшен режиме:**
```bash
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up --build -d
```

#### Переменные окружения для Docker

**Обязательные переменные:**
- `TELEGRAM_BOT_TOKEN` - токен бота от BotFather
- `TELEGRAM_SECRET_TOKEN` - секретный токен для webhook

**Опциональные переменные:**
- `ASPNETCORE_ENVIRONMENT` - среда выполнения (Development/Production)
- `ASPNETCORE_HTTP_PORTS` - HTTP порт (по умолчанию 8080)
- `ASPNETCORE_HTTPS_PORTS` - HTTPS порт (по умолчанию 8081)

#### Примеры использования

**Запуск с переменными окружения:**
```bash
# Windows
set TELEGRAM_BOT_TOKEN=7642287932:AAGAaP0BdJgvxrE3UEfdAoDcrJ0D9TzmSJI
set TELEGRAM_SECRET_TOKEN=my-secret-token
docker-compose up --build

# Linux/Mac
export TELEGRAM_BOT_TOKEN=7642287932:AAGAaP0BdJgvxrE3UEfdAoDcrJ0D9TzmSJI
export TELEGRAM_SECRET_TOKEN=my-secret-token
docker-compose up --build
```

**Запуск в фоновом режиме:**
```bash
docker-compose up --build -d
```

**Просмотр логов:**
```bash
docker-compose logs -f youdovezu.presentation
```

**Остановка:**
```bash
docker-compose down
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
