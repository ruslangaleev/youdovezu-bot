# Настройка pgAdmin для администрирования PostgreSQL

## Описание

pgAdmin - это веб-интерфейс для администрирования PostgreSQL баз данных. Он позволяет просматривать данные, выполнять SQL запросы, управлять схемой базы данных и многое другое.

## Запуск

pgAdmin автоматически запускается вместе с остальными сервисами при выполнении:

```bash
docker-compose up -d
```

## Доступ к pgAdmin

После запуска pgAdmin будет доступен по адресу:
- **URL**: http://localhost:5050
- **Email**: admin@youdovezu.com (по умолчанию)
- **Пароль**: admin123 (по умолчанию)

## Автоматическое подключение к базе данных

Подключение к базе данных настроено автоматически! 

После входа в pgAdmin вы сразу увидите сервер **"YouDovezu Database"** в левой панели. Просто нажмите на него и введите пароль для доступа к базе данных при первом подключении:
- **Пароль**: youdovezu_password

pgAdmin автоматически запомнит пароль для последующих подключений.

### Настройки подключения (предустановлены)

- **Host name/address**: postgres
- **Port**: 5432
- **Maintenance database**: youdovezu
- **Username**: youdovezu
- **Password**: youdovezu_password

Если по какой-то причине сервер не появился автоматически, вы можете добавить его вручную:

1. **Правый клик** на "Servers" в левой панели
2. Выберите **"Register" → "Server..."**
3. Заполните поля, используя настройки выше
4. Нажмите **"Save"**

## Использование

После подключения вы сможете:

- **Просматривать таблицы**: Разверните сервер → Databases → youdovezu → Schemas → public → Tables
- **Выполнять SQL запросы**: Используйте Query Tool (SQL иконка в верхней панели)
- **Просматривать данные**: Правый клик на таблице → "View/Edit Data" → "All Rows"
- **Управлять схемой**: Создавать, изменять и удалять таблицы, индексы и т.д.

## Полезные SQL запросы

### Просмотр всех пользователей
```sql
SELECT * FROM "Users" ORDER BY "CreatedAt" DESC;
```

### Просмотр пользователей с подтвержденным номером телефона
```sql
SELECT "Id", "TelegramId", "FirstName", "LastName", "PhoneNumber", "PrivacyConsent", "CanBePassenger", "Status"
FROM "Users" 
WHERE "PhoneNumber" IS NOT NULL AND "PhoneNumber" != '';
```

### Статистика регистрации
```sql
SELECT 
    COUNT(*) as total_users,
    COUNT(CASE WHEN "PrivacyConsent" = true THEN 1 END) as privacy_consent_given,
    COUNT(CASE WHEN "PhoneNumber" IS NOT NULL AND "PhoneNumber" != '' THEN 1 END) as phone_confirmed,
    COUNT(CASE WHEN "CanBePassenger" = true THEN 1 END) as can_be_passenger
FROM "Users";
```

## Переменные окружения

Вы можете настроить pgAdmin через переменные окружения в файле `.env`:

```env
# pgAdmin Configuration
PGADMIN_EMAIL=your-email@example.com
PGADMIN_PASSWORD=your-secure-password
```

## Безопасность

⚠️ **Важно**: В продакшене обязательно измените пароли по умолчанию!

- Измените `PGADMIN_PASSWORD` на сложный пароль
- Используйте `PGADMIN_EMAIL` с реальным email адресом
- Рассмотрите возможность ограничения доступа к pgAdmin только для администраторов

## Порты

- **pgAdmin**: 5050 (веб-интерфейс)
- **PostgreSQL**: 5432 (прямое подключение к БД)

## Troubleshooting

### pgAdmin не запускается
```bash
# Проверьте логи
docker-compose logs pgadmin

# Перезапустите сервис
docker-compose restart pgadmin
```

### Не удается подключиться к базе данных
- Убедитесь, что PostgreSQL запущен: `docker-compose ps`
- Проверьте, что используете правильные учетные данные
- Убедитесь, что в Host name/address указано `postgres` (имя сервиса в docker-compose)
