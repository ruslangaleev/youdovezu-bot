#!/bin/bash
# Скрипт для назначения роли администратора пользователю
# Использование: ./set-admin-role.sh

docker exec -i youdovezu-postgres psql -U youdovezu -d youdovezu << EOF
-- Устанавливаем роль администратора для пользователя с Telegram ID 315605633
UPDATE "Users"
SET "SystemRole" = 2  -- 2 = Admin (SystemRole.Admin)
WHERE "TelegramId" = 315605633;

-- Проверяем результат
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
EOF


