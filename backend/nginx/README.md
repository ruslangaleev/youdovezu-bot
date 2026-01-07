# Настройка Nginx и Let's Encrypt

## Первоначальная настройка SSL сертификатов

### 1. Убедитесь, что домен указывает на ваш сервер

Проверьте, что DNS записи для `youdovezu.ru` и `www.youdovezu.ru` указывают на IP адрес вашего сервера:

```bash
dig youdovezu.ru
dig www.youdovezu.ru
```

### 2. Запустите инициализацию Let's Encrypt

На сервере в директории `backend/` выполните:

```bash
# Сделайте скрипт исполняемым
chmod +x nginx/init-letsencrypt.sh

# Запустите инициализацию
./nginx/init-letsencrypt.sh
```

**Важно:** Перед запуском скрипта:
- Убедитесь, что порты 80 и 443 открыты в firewall
- Убедитесь, что домен указывает на сервер
- Измените email в скрипте `init-letsencrypt.sh` на ваш реальный email

### 3. Проверка работы

После успешной инициализации проверьте:

```bash
# Проверьте статус контейнеров
docker compose -f docker-compose.prod.yml ps

# Проверьте сертификаты
docker compose -f docker-compose.prod.yml exec certbot certbot certificates

# Проверьте доступность через HTTPS
curl -I https://youdovezu.ru
```

## Автоматическое обновление сертификатов

Сертификаты Let's Encrypt действительны 90 дней. Certbot контейнер автоматически обновляет их каждые 12 часов.

### Проверка автоматического обновления

```bash
# Проверьте логи certbot
docker compose -f docker-compose.prod.yml logs certbot

# Вручную запустите обновление (для тестирования)
docker compose -f docker-compose.prod.yml exec certbot certbot renew --webroot --webroot-path=/var/www/certbot
```

## Структура файлов

```
backend/
├── nginx/
│   ├── nginx.conf              # Основная конфигурация Nginx
│   ├── conf.d/
│   │   └── youdovezu.conf      # Конфигурация для домена
│   ├── init-letsencrypt.sh     # Скрипт первоначальной настройки
│   └── renew-certs.sh          # Скрипт для ручного обновления
└── docker-compose.prod.yml     # Docker Compose конфигурация
```

## Устранение проблем

### Проблема: "Failed to obtain certificate"

**Причины:**
- Домен не указывает на сервер
- Порт 80 закрыт в firewall
- Nginx не запущен

**Решение:**
1. Проверьте DNS записи
2. Откройте порты 80 и 443
3. Убедитесь, что nginx контейнер запущен

### Проблема: Сертификат не обновляется

**Решение:**
```bash
# Проверьте логи
docker compose -f docker-compose.prod.yml logs certbot

# Запустите обновление вручную
docker compose -f docker-compose.prod.yml exec certbot certbot renew --force-renewal
```

### Проблема: Nginx не перезагружается после обновления сертификата

**Решение:**
```bash
# Перезагрузите nginx вручную
docker compose -f docker-compose.prod.yml exec nginx nginx -s reload
```

## Безопасность

- ✅ Все HTTP запросы перенаправляются на HTTPS
- ✅ Используются современные SSL/TLS протоколы (TLSv1.2, TLSv1.3)
- ✅ Настроены security headers
- ✅ Включен rate limiting для API
- ✅ Сертификаты обновляются автоматически


