# Документация по развертыванию

Эта директория содержит документацию по развертыванию проекта YouDovezu Bot.

## Быстрый старт

1. **Настройка GitHub Secrets** - см. [github-actions-setup.md](./github-actions-setup.md)
2. **Настройка VPS сервера** - см. [github-actions-setup.md](./github-actions-setup.md)
3. **Запуск деплоя** - Push в ветку `main` или `master`, или запуск вручную через GitHub Actions UI

## Файлы документации

- **[github-actions-setup.md](./github-actions-setup.md)** - Подробная инструкция по настройке автоматического деплоя через GitHub Actions

## Workflow файлы

- **`.github/workflows/deploy-production.yml`** - GitHub Actions workflow для автоматического развертывания на VPS

## Процесс деплоя

1. **Сборка фронтенда** - React приложение собирается для продакшена
2. **Подготовка backend** - Фронтенд копируется в `wwwroot` backend проекта
3. **Сборка backend** - .NET решение собирается
4. **Деплой на VPS** - Файлы копируются на сервер через SSH/rsync
5. **Сборка Docker образов** - Docker образы собираются на сервере
6. **Запуск сервисов** - Docker Compose запускает контейнеры
7. **Health check** - Проверка работоспособности приложения

## Требования

- VPS сервер с установленными Docker и Docker Compose
- SSH доступ к серверу
- GitHub репозиторий с настроенными секретами

## Поддержка

При возникновении проблем см. раздел "Устранение неполадок" в [github-actions-setup.md](./github-actions-setup.md)

