# Документация по развертыванию

Эта директория содержит документацию по развертыванию проекта YouDovezu Bot.

## Варианты развертывания

### 1. Docker Compose (legacy)

Развертывание через Docker Compose на VPS сервере.

**Документация:**
- **[github-actions-setup.md](./github-actions-setup.md)** - Подробная инструкция по настройке автоматического деплоя через GitHub Actions

**Workflow:**
- **`.github/workflows/deploy-production.yml`** - GitHub Actions workflow для автоматического развертывания на VPS

**Процесс:**
1. Сборка фронтенда - React приложение собирается для продакшена
2. Подготовка backend - Фронтенд копируется в `wwwroot` backend проекта
3. Сборка backend - .NET решение собирается
4. Деплой на VPS - Файлы копируются на сервер через SSH/tar
5. Сборка Docker образов - Docker образы собираются на сервере
6. Запуск сервисов - Docker Compose запускает контейнеры
7. Health check - Проверка работоспособности приложения

**Требования:**
- VPS сервер с установленными Docker и Docker Compose
- SSH доступ к серверу
- GitHub репозиторий с настроенными секретами

## Общие файлы документации

- **[github-secrets-guide.md](./github-secrets-guide.md)** - Руководство по настройке GitHub Secrets
- **[ssh-key-setup-simple.md](./ssh-key-setup-simple.md)** - Упрощенная инструкция по SSH ключам

## Поддержка

При возникновении проблем:
- Для Docker Compose: см. раздел "Устранение неполадок" в [github-actions-setup.md](./github-actions-setup.md)

