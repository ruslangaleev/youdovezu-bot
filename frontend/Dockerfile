FROM node:22-alpine AS builder

WORKDIR /app

# Build arguments для переменных окружения
ARG REACT_APP_API_URL
ARG REACT_APP_WEBAPP_URL
ARG REACT_APP_TEST_MODE=false
ARG NODE_ENV=production

# Копируем файлы зависимостей для кэширования
COPY package*.json ./
RUN npm ci --frozen-lockfile

# Копируем ВСЕ файлы проекта
COPY . .

# Создаем .env файл из build args (если переданы) или используем env.production
RUN set -e && \
    if [ -n "$REACT_APP_API_URL" ]; then \
      echo "REACT_APP_API_URL=$REACT_APP_API_URL" > .env; \
      echo "REACT_APP_WEBAPP_URL=${REACT_APP_WEBAPP_URL:-http://localhost:3000}" >> .env; \
      echo "REACT_APP_TEST_MODE=${REACT_APP_TEST_MODE:-false}" >> .env; \
      echo "NODE_ENV=${NODE_ENV:-production}" >> .env; \
    else \
      [ -f env.production ] && cp env.production .env || true; \
    fi

# Выполняем production-сборку
RUN npm run build

# Production stage с nginx
FROM nginx:alpine

# Копируем собранное приложение
COPY --from=builder /app/build /usr/share/nginx/html

# Копируем конфигурацию nginx (если есть, иначе используем дефолтную)
# COPY nginx.conf /etc/nginx/conf.d/default.conf

# Nginx по умолчанию слушает на порту 80
EXPOSE 80

# Nginx запускается автоматически при старте контейнера
CMD ["nginx", "-g", "daemon off;"]