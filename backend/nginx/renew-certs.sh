#!/bin/sh

# Скрипт для обновления сертификатов Let's Encrypt
# Этот скрипт запускается автоматически через cron или systemd timer

docker compose -f docker-compose.prod.yml exec certbot certbot renew --webroot --webroot-path=/var/www/certbot --quiet

# Перезагружаем nginx после обновления сертификатов
docker compose -f docker-compose.prod.yml exec nginx nginx -s reload


