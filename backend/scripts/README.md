# Скрипты для управления базой данных

## Назначение роли администратора

### Способ 1: Через SQL файл (рекомендуется)

```bash
# Linux/Mac
cat scripts/set-admin-role.sql | docker exec -i youdovezu-postgres psql -U youdovezu -d youdovezu

# Windows PowerShell
Get-Content scripts\set-admin-role.sql | docker exec -i youdovezu-postgres psql -U youdovezu -d youdovezu
```

### Способ 2: Через pgAdmin

1. Откройте pgAdmin: http://localhost:5050
2. Подключитесь к серверу "YouDovezu Database"
3. Откройте Query Tool
4. Выполните SQL из файла `scripts/set-admin-role.sql`

### Способ 3: Через bash скрипт (Linux/Mac)

```bash
chmod +x scripts/set-admin-role.sh
./scripts/set-admin-role.sh
```

### Способ 4: Через PowerShell скрипт (Windows)

```powershell
.\scripts\set-admin-role.ps1
```

## Проверка назначенной роли

После выполнения скрипта вы увидите результат с информацией о пользователе:
- **TelegramId**: 315605633
- **SystemRole**: 2 (Admin)
- **RoleName**: Admin

## Назначение роли другому пользователю

Чтобы назначить роль администратора другому пользователю, отредактируйте файл `set-admin-role.sql` и измените Telegram ID в строке:

```sql
WHERE "TelegramId" = 315605633;
```

Замените `315605633` на нужный Telegram ID пользователя.


