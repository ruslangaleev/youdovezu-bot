# Настройка k3s и Self-Hosted Runner для деплоя

Этот документ описывает процесс настройки k3s и GitHub Actions self-hosted runner для автоматического развертывания проекта.

## Предварительные требования

### На сервере должны быть установлены:

1. **k3s** (версия 1.24 или выше)
2. **Docker** (для сборки образов)
3. **kubectl** (для управления кластером)
4. **Git** (для клонирования репозитория)

## Установка k3s

### 1. Установка k3s на сервере

```bash
# Установка k3s
curl -sfL https://get.k3s.io | sh -

# Проверка установки
sudo k3s kubectl get nodes

# Настройка kubectl для работы без sudo
mkdir -p ~/.kube
sudo cp /etc/rancher/k3s/k3s.yaml ~/.kube/config
sudo chown $USER ~/.kube/config
```

### 2. Проверка работы k3s

```bash
# Проверка статуса
sudo systemctl status k3s

# Проверка узлов
kubectl get nodes

# Проверка подов
kubectl get pods --all-namespaces
```

## Настройка Self-Hosted Runner

### 1. Создание runner на GitHub

1. Перейдите в ваш репозиторий на GitHub
2. Перейдите в **Settings** → **Actions** → **Runners**
3. Нажмите **New self-hosted runner**
4. Выберите операционную систему (Linux)
5. Скопируйте команды для установки

### 2. Установка runner на сервере

```bash
# Создайте директорию для runner
mkdir -p ~/actions-runner && cd ~/actions-runner

# Скачайте и распакуйте runner (команды из GitHub)
curl -o actions-runner-linux-x64-2.311.0.tar.gz -L https://github.com/actions/runner/releases/download/v2.311.0/actions-runner-linux-x64-2.311.0.tar.gz
tar xzf ./actions-runner-linux-x64-2.311.0.tar.gz

# Настройте runner (используйте токен из GitHub)
./config.sh --url https://github.com/ваш-username/ваш-репозиторий --token ВАШ_ТОКЕН

# Запустите runner как сервис
sudo ./svc.sh install
sudo ./svc.sh start

# Проверка статуса
sudo ./svc.sh status
```

### 3. Настройка прав доступа

Runner должен иметь доступ к:
- Docker (для сборки образов)
- kubectl (для применения манифестов)
- k3s (для загрузки образов)

```bash
# Добавьте пользователя runner в группу docker
sudo usermod -aG docker $USER

# Настройте права для kubectl
sudo chown $USER ~/.kube/config
chmod 600 ~/.kube/config

# Проверьте доступ к k3s
sudo k3s kubectl get nodes
```

## Настройка GitHub Secrets

Добавьте следующие секреты в GitHub (Settings → Secrets and variables → Actions):

| Secret Name | Описание |
|------------|----------|
| `TELEGRAM_BOT_TOKEN` | Токен бота от BotFather |
| `TELEGRAM_SECRET_TOKEN` | Секретный токен для webhook |

## Первоначальная настройка кластера

### 1. Создание namespace

```bash
kubectl apply -f k8s/namespace.yaml
```

### 2. Создание секретов

```bash
# PostgreSQL секрет
kubectl apply -f k8s/postgres-secret.yaml

# App секрет (будет обновлен автоматически через workflow)
kubectl apply -f k8s/app-secret.yaml
```

### 3. Развертывание PostgreSQL

```bash
kubectl apply -f k8s/postgres-statefulset.yaml

# Ждем готовности
kubectl wait --for=condition=ready pod -l app=postgres -n youdovezu --timeout=300s
```

## Проверка развертывания

### Просмотр статуса

```bash
# Просмотр подов
kubectl get pods -n youdovezu

# Просмотр сервисов
kubectl get services -n youdovezu

# Просмотр логов приложения
kubectl logs -n youdovezu deployment/youdovezu-app -f
```

### Проверка health endpoint

```bash
# Port-forward для доступа к сервису
kubectl port-forward -n youdovezu service/youdovezu-app 8080:80

# В другом терминале
curl http://localhost:8080/api/bot/health
```

## Обновление секретов

Секреты обновляются автоматически при каждом деплое из GitHub Secrets. Если нужно обновить вручную:

```bash
kubectl create secret generic app-secret \
  --from-literal=Telegram__BotToken="ваш_токен" \
  --from-literal=Telegram__SecretToken="ваш_секрет" \
  -n youdovezu \
  --dry-run=client -o yaml | kubectl apply -f -
```

## Масштабирование

### Увеличение количества реплик приложения

```bash
kubectl scale deployment youdovezu-app -n youdovezu --replicas=3
```

Или отредактируйте `k8s/app-deployment.yaml` и измените `replicas: 1` на нужное значение.

## Устранение неполадок

### Проблема: Runner не запускается

```bash
# Проверьте статус
sudo ./svc.sh status

# Просмотрите логи
sudo journalctl -u actions.runner.* -f
```

### Проблема: Не могу загрузить образ в k3s

```bash
# Проверьте доступ к k3s
sudo k3s ctr images list

# Попробуйте загрузить образ вручную
sudo k3s ctr images import /path/to/image.tar
```

### Проблема: Pod не запускается

```bash
# Просмотрите описание пода
kubectl describe pod <pod-name> -n youdovezu

# Просмотрите логи
kubectl logs <pod-name> -n youdovezu

# Проверьте события
kubectl get events -n youdovezu --sort-by='.lastTimestamp'
```

### Проблема: PersistentVolume не создается

k3s использует local-path provisioner по умолчанию. Проверьте:

```bash
# Проверьте StorageClass
kubectl get storageclass

# Проверьте PVC
kubectl get pvc -n youdovezu

# Проверьте PV
kubectl get pv
```

## Полезные команды

```bash
# Перезапуск deployment
kubectl rollout restart deployment/youdovezu-app -n youdovezu

# Просмотр всех ресурсов в namespace
kubectl get all -n youdovezu

# Удаление всех ресурсов в namespace
kubectl delete all --all -n youdovezu

# Просмотр конфигурации deployment
kubectl get deployment youdovezu-app -n youdovezu -o yaml

# Редактирование deployment
kubectl edit deployment youdovezu-app -n youdovezu
```

## Безопасность

### Рекомендации:

1. ✅ Используйте отдельного пользователя для runner
2. ✅ Ограничьте права доступа runner только необходимыми
3. ✅ Регулярно обновляйте k3s и runner
4. ✅ Используйте секреты для хранения токенов
5. ✅ Настройте сетевые политики (NetworkPolicies) для изоляции подов
6. ✅ Используйте RBAC для ограничения доступа

## Дополнительные настройки

### Настройка Ingress (опционально)

Если нужен внешний доступ через домен:

```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: youdovezu-ingress
  namespace: youdovezu
spec:
  rules:
  - host: api.youdovezu.com
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: youdovezu-app
            port:
              number: 80
```

### Настройка мониторинга (опционально)

Можно добавить Prometheus и Grafana для мониторинга:

```bash
# Установка Prometheus Operator
kubectl apply -f https://raw.githubusercontent.com/prometheus-operator/prometheus-operator/main/bundle.yaml
```

## Поддержка

При возникновении проблем:
1. Проверьте логи runner: `sudo journalctl -u actions.runner.* -f`
2. Проверьте логи подов: `kubectl logs -n youdovezu <pod-name>`
3. Проверьте события: `kubectl get events -n youdovezu`
4. Создайте issue в репозитории с описанием проблемы

