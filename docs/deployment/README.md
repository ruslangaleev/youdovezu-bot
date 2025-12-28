# Документация по развертыванию

Эта директория содержит документацию по развертыванию проекта YouDovezu Bot.

## Варианты развертывания

### 1. k3s (рекомендуется)

Развертывание в Kubernetes кластер k3s с использованием self-hosted runner.

**Документация:**
- **[k3s-setup.md](./k3s-setup.md)** - Подробная инструкция по настройке k3s и self-hosted runner
- **[k8s/README.md](../k8s/README.md)** - Описание Kubernetes манифестов

**Workflow:**
- **`.github/workflows/deploy-k3s.yml`** - GitHub Actions workflow для развертывания в k3s

**Процесс:**
1. Сборка фронтенда и backend
2. Сборка Docker образа
3. Загрузка образа в k3s (локально)
4. Применение Kubernetes манифестов
5. Health check

**Требования:**
- k3s установлен на сервере
- Self-hosted runner настроен
- Docker для сборки образов
- kubectl для управления кластером

### 2. Docker Compose (legacy)

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
- Для k3s: см. раздел "Устранение неполадок" в [k3s-setup.md](./k3s-setup.md)
- Для Docker Compose: см. раздел "Устранение неполадок" в [github-actions-setup.md](./github-actions-setup.md)

