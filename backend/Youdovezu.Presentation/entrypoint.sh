#!/bin/bash
set -e

# Создаем директорию uploads с правильными правами
mkdir -p /app/uploads/driver-documents
chmod -R 777 /app/uploads

# Запускаем приложение
exec dotnet Youdovezu.Presentation.dll


