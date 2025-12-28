# Настройка GitHub Actions для деплоя на VPS

Этот документ описывает процесс настройки автоматического развертывания проекта на VPS сервер через GitHub Actions.

## Предварительные требования

### На VPS сервере должны быть установлены:

1. **Docker** (версия 20.10 или выше)
2. **Docker Compose** (версия 2.0 или выше)
3. **SSH сервер** с доступом по ключу
4. **Пользователь с правами на запуск Docker** (обычно добавление в группу `docker`)

### Проверка установки на VPS:

```bash
# Проверка Docker
docker --version
docker-compose --version

# Проверка SSH
systemctl status sshd
```

## Настройка GitHub Secrets

### 1. Перейдите в настройки репозитория

1. Откройте ваш репозиторий на GitHub
2. Перейдите в **Settings** → **Secrets and variables** → **Actions**
3. Нажмите **New repository secret**

### 2. Добавьте следующие секреты:

| Secret Name | Описание | Пример значения |
|------------|----------|----------------|
| `VPS_HOST` | IP адрес или домен VPS сервера | `192.168.1.100` или `yourserver.com` |
| `VPS_USER` | Имя пользователя для SSH подключения | `deploy` или `root` |
| `VPS_SSH_PRIVATE_KEY` | Приватный SSH ключ для подключения к серверу | См. инструкцию ниже |
| `VPS_DEPLOY_PATH` | (Опционально) Путь для развертывания на сервере | `/opt/youdovezu-bot` (по умолчанию) |

### 3. Генерация SSH ключа

#### На вашем локальном компьютере:

```bash
# Генерация новой пары ключей (если ещё нет)
ssh-keygen -t ed25519 -C "github-actions-deploy" -f ~/.ssh/github_actions_deploy

# Или используйте RSA (если ed25519 не поддерживается)
ssh-keygen -t rsa -b 4096 -C "github-actions-deploy" -f ~/.ssh/github_actions_deploy
```

#### Копирование публичного ключа на VPS:

```bash
# Скопируйте публичный ключ на сервер
ssh-copy-id -i ~/.ssh/github_actions_deploy.pub user@your-server-ip

# Или вручную:
cat ~/.ssh/github_actions_deploy.pub | ssh user@your-server-ip "mkdir -p ~/.ssh && cat >> ~/.ssh/authorized_keys && chmod 600 ~/.ssh/authorized_keys"
```

#### Добавление приватного ключа в GitHub Secrets:

```bash
# Скопируйте содержимое приватного ключа
cat ~/.ssh/github_actions_deploy

# Вставьте ВСЁ содержимое (включая строки -----BEGIN ... и -----END ...) 
# в поле значения секрета VPS_SSH_PRIVATE_KEY в GitHub
```

**Важно:** 
- Никогда не коммитьте приватные ключи в репозиторий!
- Приватный ключ должен начинаться с `-----BEGIN` и заканчиваться `-----END`

## Настройка VPS сервера

### 1. Создание пользователя для деплоя (рекомендуется)

```bash
# Создайте пользователя
sudo adduser deploy

# Добавьте пользователя в группу docker
sudo usermod -aG docker deploy

# Создайте директорию для проекта
sudo mkdir -p /opt/youdovezu-bot
sudo chown deploy:deploy /opt/youdovezu-bot
```

### 2. Настройка прав доступа

```bash
# Убедитесь, что пользователь может запускать Docker без sudo
sudo usermod -aG docker $USER

# Проверьте права
groups
# Должна быть группа 'docker'
```

### 3. Создание файла .env на сервере

```bash
# На сервере создайте файл .env в директории проекта
cd /opt/youdovezu-bot/backend
nano .env
```

**Содержимое .env файла:**

```bash
# Telegram Bot Configuration
Telegram__BotToken=your-production-bot-token-here
Telegram__SecretToken=your-production-secret-token-here

# Environment
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_HTTP_PORTS=8080
ASPNETCORE_HTTPS_PORTS=8081

# Database (если используете внешнюю БД, а не Docker Compose)
# Database__ConnectionString=Host=your-db-host;Port=5432;Database=youdovezu;Username=user;Password=password

# pgAdmin (опционально)
PGADMIN_EMAIL=admin@youdovezu.com
PGADMIN_PASSWORD=your-secure-password
```

**Важно:** 
- Не коммитьте файл `.env` в git!
- Используйте сильные пароли для продакшена
- Храните `.env` файл в безопасном месте

## Настройка Firewall

### Открытие необходимых портов:

```bash
# Если используете UFW (Ubuntu)
sudo ufw allow 22/tcp    # SSH
sudo ufw allow 8080/tcp  # HTTP для приложения
sudo ufw allow 8081/tcp  # HTTPS для приложения (если используется)
sudo ufw enable

# Если используете firewalld (CentOS/RHEL)
sudo firewall-cmd --permanent --add-port=22/tcp
sudo firewall-cmd --permanent --add-port=8080/tcp
sudo firewall-cmd --permanent --add-port=8081/tcp
sudo firewall-cmd --reload
```

## Настройка Reverse Proxy (опционально, но рекомендуется)

Для использования домена и HTTPS рекомендуется настроить Nginx или другой reverse proxy:

### Пример конфигурации Nginx:

```nginx
server {
    listen 80;
    server_name your-domain.com;

    location / {
        proxy_pass http://localhost:8080;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }
}
```

## Запуск деплоя

### Автоматический деплой

Workflow автоматически запускается при:
- Push в ветку `main` или `master`
- Создании Pull Request в эти ветки (если настроено)

### Ручной запуск

1. Перейдите в **Actions** в вашем репозитории
2. Выберите workflow **Deploy to Production VPS**
3. Нажмите **Run workflow**
4. Выберите ветку и нажмите **Run workflow**

## Мониторинг деплоя

### Просмотр логов в GitHub Actions:

1. Перейдите в **Actions** → выберите последний запуск
2. Откройте job **build-and-deploy**
3. Просмотрите логи каждого шага

### Просмотр логов на сервере:

```bash
# Подключитесь к серверу
ssh user@your-server-ip

# Перейдите в директорию проекта
cd /opt/youdovezu-bot/backend

# Просмотр логов контейнеров
docker-compose -f docker-compose.yml -f docker-compose.prod.yml logs -f

# Просмотр логов конкретного сервиса
docker-compose -f docker-compose.yml -f docker-compose.prod.yml logs -f youdovezu.presentation
```

## Проверка работоспособности

### Health Check:

```bash
# На сервере
curl http://localhost:8080/api/bot/health

# Или с внешнего компьютера (если порт открыт)
curl http://your-server-ip:8080/api/bot/health
```

### Проверка статуса контейнеров:

```bash
cd /opt/youdovezu-bot/backend
docker-compose -f docker-compose.yml -f docker-compose.prod.yml ps
```

## Устранение неполадок

### Проблема: SSH подключение не работает

**Решение:**
1. Проверьте правильность `VPS_HOST` и `VPS_USER` в секретах
2. Убедитесь, что публичный ключ добавлен в `~/.ssh/authorized_keys` на сервере
3. Проверьте права на файлы: `chmod 600 ~/.ssh/authorized_keys`
4. Проверьте настройки SSH на сервере: `sudo nano /etc/ssh/sshd_config`

### Проблема: Docker команды требуют sudo

**Решение:**
```bash
# Добавьте пользователя в группу docker
sudo usermod -aG docker $USER

# Выйдите и войдите снова, или выполните:
newgrp docker
```

### Проблема: Health check не проходит

**Решение:**
1. Проверьте логи контейнеров: `docker-compose logs`
2. Убедитесь, что порт 8080 открыт
3. Проверьте переменные окружения в `.env` файле
4. Проверьте подключение к базе данных

### Проблема: Frontend не отображается

**Решение:**
1. Убедитесь, что frontend собран: проверьте наличие файлов в `wwwroot`
2. Проверьте, что `app.UseStaticFiles()` включен в `Program.cs`
3. Проверьте логи контейнера на ошибки

## Безопасность

### Рекомендации:

1. ✅ Используйте отдельного пользователя для деплоя (не root)
2. ✅ Ограничьте SSH доступ по IP (если возможно)
3. ✅ Используйте сильные пароли и секретные токены
4. ✅ Регулярно обновляйте Docker и систему
5. ✅ Настройте автоматические бэкапы базы данных
6. ✅ Используйте HTTPS через reverse proxy
7. ✅ Настройте мониторинг и алерты
8. ✅ Ограничьте доступ к портам только необходимым IP

## Дополнительные настройки

### Автоматические бэкапы базы данных:

Создайте cron job для регулярных бэкапов:

```bash
# Добавьте в crontab
crontab -e

# Бэкап каждый день в 2:00
0 2 * * * docker exec youdovezu-postgres pg_dump -U youdovezu youdovezu > /opt/backups/youdovezu_$(date +\%Y\%m\%d).sql
```

### Мониторинг дискового пространства:

```bash
# Добавьте проверку в cron
0 * * * * df -h | awk '$5 > 80 {print "Disk usage warning: " $0}' | mail -s "Disk Usage Alert" admin@example.com
```

## Поддержка

При возникновении проблем:
1. Проверьте логи GitHub Actions
2. Проверьте логи на сервере
3. Проверьте документацию Docker и Docker Compose
4. Создайте issue в репозитории с описанием проблемы

