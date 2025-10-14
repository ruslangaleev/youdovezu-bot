# Инструкции по развертыванию Telegram Bot

## Быстрый старт

### 1. Подготовка

1. **Скопируйте файл конфигурации:**
```bash
cp env.example .env
```

2. **Заполните переменные в `.env`:**
```bash
TELEGRAM_BOT_TOKEN=your-real-bot-token-from-botfather
TELEGRAM_SECRET_TOKEN=your-secret-token-min-16-chars
```

### 2. Запуск

#### Локальная разработка
```bash
docker-compose up --build
```

#### Продакшен
```bash
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up --build -d
```

### 3. Настройка Webhook

После запуска приложения настройте webhook:

```bash
curl -X POST "https://api.telegram.org/bot<YOUR_BOT_TOKEN>/setWebhook" \
     -H "Content-Type: application/json" \
     -d '{"url": "https://your-domain.com/api/bot/webhook", "secret_token": "your-secret-token"}'
```

## Переменные окружения

| Переменная | Описание | Обязательная | По умолчанию |
|------------|----------|--------------|--------------|
| `TELEGRAM_BOT_TOKEN` | Токен бота от BotFather | ✅ | - |
| `TELEGRAM_SECRET_TOKEN` | Секретный токен для webhook | ✅ | - |
| `ASPNETCORE_ENVIRONMENT` | Среда выполнения | ❌ | Development |
| `ASPNETCORE_HTTP_PORTS` | HTTP порт | ❌ | 8080 |
| `ASPNETCORE_HTTPS_PORTS` | HTTPS порт | ❌ | 8081 |

## Полезные команды

```bash
# Просмотр логов
docker-compose logs -f youdovezu.presentation

# Остановка
docker-compose down

# Перезапуск
docker-compose restart youdovezu.presentation

# Проверка статуса
docker-compose ps

# Проверка health check
curl http://localhost:8080/api/bot/health
```

## Безопасность

- ✅ Используйте переменные окружения для токенов
- ✅ Не коммитьте файл `.env` в git
- ✅ Используйте HTTPS для webhook
- ✅ Регулярно обновляйте секретные токены
- ✅ Мониторьте логи на предмет подозрительной активности
