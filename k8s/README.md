# Kubernetes манифесты для YouDovezu

Этот каталог содержит Kubernetes манифесты для развертывания приложения в k3s.

## Структура манифестов

```
k8s/
├── namespace.yaml              # Namespace для приложения
├── postgres-secret.yaml        # Секреты для PostgreSQL
├── postgres-statefulset.yaml   # PostgreSQL StatefulSet и Service
├── app-secret.yaml             # Секреты для приложения (токены)
├── app-configmap.yaml          # ConfigMap с конфигурацией
├── app-pvc.yaml                # PersistentVolumeClaim для uploads
├── app-deployment.yaml         # Deployment приложения
├── app-service.yaml            # Service для приложения
└── pgadmin-deployment.yaml     # pgAdmin (опционально)
```

## Порядок применения манифестов

### 1. Создание namespace

```bash
kubectl apply -f k8s/namespace.yaml
```

### 2. Развертывание PostgreSQL

```bash
kubectl apply -f k8s/postgres-secret.yaml
kubectl apply -f k8s/postgres-statefulset.yaml

# Ждем готовности
kubectl wait --for=condition=ready pod -l app=postgres -n youdovezu --timeout=300s
```

### 3. Развертывание приложения

```bash
# Сначала создаем секреты (обновите значения!)
kubectl apply -f k8s/app-secret.yaml

# Затем остальные ресурсы
kubectl apply -f k8s/app-pvc.yaml
kubectl apply -f k8s/app-configmap.yaml
kubectl apply -f k8s/app-deployment.yaml
kubectl apply -f k8s/app-service.yaml
```

### 4. Развертывание pgAdmin (опционально)

```bash
kubectl apply -f k8s/pgadmin-deployment.yaml
```

## Обновление секретов

### Обновление секретов приложения

```bash
kubectl create secret generic app-secret \
  --from-literal=Telegram__BotToken="ваш_токен" \
  --from-literal=Telegram__SecretToken="ваш_секрет" \
  -n youdovezu \
  --dry-run=client -o yaml | kubectl apply -f -
```

### Обновление секретов PostgreSQL

```bash
kubectl create secret generic postgres-secret \
  --from-literal=password="новый_пароль" \
  -n youdovezu \
  --dry-run=client -o yaml | kubectl apply -f -
```

После обновления пароля PostgreSQL нужно обновить ConfigMap с connection string:

```bash
kubectl edit configmap app-config -n youdovezu
```

## Масштабирование

### Увеличение количества реплик

Отредактируйте `app-deployment.yaml`:

```yaml
spec:
  replicas: 3  # Измените на нужное количество
```

Или используйте команду:

```bash
kubectl scale deployment youdovezu-app -n youdovezu --replicas=3
```

## Обновление образа

При обновлении образа через workflow, deployment автоматически перезапустится. Для ручного обновления:

```bash
# Загрузите новый образ в k3s
sudo k3s ctr images import /path/to/image.tar

# Перезапустите deployment
kubectl rollout restart deployment/youdovezu-app -n youdovezu
```

## Удаление развертывания

```bash
# Удаление всех ресурсов в namespace
kubectl delete all --all -n youdovezu
kubectl delete pvc --all -n youdovezu
kubectl delete secrets --all -n youdovezu
kubectl delete configmaps --all -n youdovezu

# Удаление namespace
kubectl delete namespace youdovezu
```

## Проверка статуса

```bash
# Все ресурсы в namespace
kubectl get all -n youdovezu

# Логи приложения
kubectl logs -n youdovezu deployment/youdovezu-app -f

# Описание deployment
kubectl describe deployment youdovezu-app -n youdovezu

# События
kubectl get events -n youdovezu --sort-by='.lastTimestamp'
```

## Доступ к сервисам

### Port-forward для локального доступа

```bash
# Приложение
kubectl port-forward -n youdovezu service/youdovezu-app 8080:80

# pgAdmin
kubectl port-forward -n youdovezu service/pgadmin 5050:80
```

### Доступ из других подов

Используйте DNS имена:
- `youdovezu-app.youdovezu.svc.cluster.local`
- `postgres.youdovezu.svc.cluster.local`
- `pgadmin.youdovezu.svc.cluster.local`

## Хранилище

Все PersistentVolumeClaim используют local-path provisioner k3s. Данные хранятся в:
- `/var/lib/rancher/k3s/storage/` (по умолчанию)

Для изменения расположения настройте StorageClass в k3s.

