# PowerShell скрипт для назначения роли администратора пользователю
# Использование: .\set-admin-role.ps1

docker exec -i youdovezu-postgres psql -U youdovezu -d youdovezu -c @"
UPDATE "Users"
SET "SystemRole" = 2
WHERE "TelegramId" = 315605633;

SELECT 
    "Id",
    "TelegramId",
    "FirstName",
    "LastName",
    "SystemRole",
    CASE 
        WHEN "SystemRole" = 0 THEN 'User'
        WHEN "SystemRole" = 1 THEN 'Moderator'
        WHEN "SystemRole" = 2 THEN 'Admin'
        ELSE 'Unknown'
    END as "RoleName"
FROM "Users"
WHERE "TelegramId" = 315605633;
"@


