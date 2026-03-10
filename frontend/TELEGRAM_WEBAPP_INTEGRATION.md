# Интеграция с Telegram WebApp

## Обзор

Веб-приложение YouDovezu полностью интегрировано с Telegram WebApp API для обеспечения безопасной авторизации и нативного пользовательского опыта.

## Компоненты интеграции

### 🔧 **Telegram WebApp SDK**

**Установка:**
```bash
npm install @twa-dev/sdk
```

**Подключение в HTML:**
```html
<script src="https://telegram.org/js/telegram-web-app.js"></script>
```

### ⚙️ **Конфигурация**

**Основные настройки в `config.ts`:**
```typescript
telegramWebApp: {
  enableSDK: true,           // Включить Telegram WebApp SDK
  showCloseButton: true,     // Показывать кнопку закрытия
  expand: true,              // Развернуть на весь экран
  showMainButton: false      // Показывать главную кнопку
}
```

### 🔐 **Авторизация через initData**

**Получение initData:**
```typescript
export const getInitData = (): string => {
  // В реальном Telegram WebApp
  if (window.Telegram?.WebApp?.initData) {
    return window.Telegram.WebApp.initData;
  }
  
  // Для тестирования
  const urlParams = new URLSearchParams(window.location.search);
  const urlInitData = urlParams.get('initData');
  
  if (urlInitData) {
    return urlInitData;
  }
  
  return '';
};
```

**Отправка на сервер:**
```typescript
const formData = new FormData();
formData.append('initData', initData);

const response = await axios.post(apiConfig.getUserInfo(), formData, apiConfig.axiosConfig);
```

### 🚀 **Инициализация WebApp**

**Функция инициализации:**
```typescript
export const initTelegramWebApp = () => {
  if (window.Telegram?.WebApp) {
    const tg = window.Telegram.WebApp;
    
    // Настройки WebApp
    if (config.telegramWebApp.expand) {
      tg.expand();
    }
    
    if (config.telegramWebApp.showCloseButton) {
      tg.enableClosingConfirmation();
    }
    
    // Настройка темы
    tg.ready();
    
    return true;
  }
  
  return false;
};
```

## Типы TypeScript

### 📝 **Расширенные типы для Telegram WebApp**

```typescript
declare global {
  interface Window {
    Telegram?: {
      WebApp?: {
        close: () => void;
        initData: string;
        initDataUnsafe: any;
        expand: () => void;
        enableClosingConfirmation: () => void;
        ready: () => void;
        platform: string;
        version: string;
        colorScheme: string;
        themeParams: any;
      };
    };
  }
}
```

## Компоненты UI

### 🎨 **TelegramWebAppInfo**

Компонент для отображения статуса интеграции:

```typescript
interface TelegramWebAppInfoProps {
  isTelegramWebApp: boolean;
}

export const TelegramWebAppInfo: React.FC<TelegramWebAppInfoProps> = ({ isTelegramWebApp }) => {
  if (!isTelegramWebApp) {
    return <div>⚠️ Тестовый режим</div>;
  }
  
  return <div>✅ Telegram WebApp</div>;
};
```

## Безопасность

### 🛡️ **Валидация initData на сервере**

Сервер проверяет подлинность initData:

```csharp
public bool ValidateInitData(string initData)
{
    // Проверяет подпись initData
    // Использует секретный ключ бота
    // Возвращает true если данные подлинные
}
```

### 🔒 **Обработка ошибок авторизации**

```typescript
if (err.response?.status === 401) {
  setError('Ошибка авторизации. Пожалуйста, откройте приложение через Telegram бота.');
} else if (err.response?.status === 400) {
  setError('Неверные данные авторизации. Пожалуйста, обновите страницу.');
}
```

## Режимы работы

### 🌐 **Telegram WebApp (продакшен)**

- **Автоматическая авторизация** через initData
- **Нативные элементы UI** (кнопки, темы)
- **Полная интеграция** с Telegram
- **Безопасная передача данных**

### 🧪 **Браузер (разработка/тестирование)**

- **Тестовые данные** через URL параметры
- **Визуальный индикатор** тестового режима
- **Отладочная информация** в консоли
- **Гибкая настройка** через переменные окружения

## Переменные окружения

### 🔧 **Настройки для тестирования**

```bash
# env.testing
REACT_APP_API_URL=http://localhost:8080
REACT_APP_WEBAPP_URL=http://localhost:3000
REACT_APP_TEST_MODE=true
REACT_APP_TEST_INIT_DATA=
NODE_ENV=development
```

### 🚀 **Настройки для продакшена**

```bash
# env.production
REACT_APP_API_URL=https://api.youdovezu.com
REACT_APP_WEBAPP_URL=https://app.youdovezu.com
REACT_APP_TEST_MODE=false
NODE_ENV=production
```

## Логирование и отладка

### 📊 **Логирование событий**

```typescript
log('Telegram WebApp initialized:', {
  platform: tg.platform,
  version: tg.version,
  colorScheme: tg.colorScheme,
  themeParams: tg.themeParams
});

log('Checking user registration with initData length:', initData.length);
log('User info received:', response.data);
```

### 🔍 **Отладочная информация**

- **Статус интеграции** - показывается в UI
- **Длина initData** - для проверки корректности
- **Ответы сервера** - для отладки API
- **Ошибки авторизации** - детальная информация

## Тестирование

### 🧪 **Локальное тестирование**

1. **Запуск в браузере:**
   ```bash
   npm start
   ```

2. **Передача тестовых данных:**
   ```
   http://localhost:3000?initData=test_data
   ```

3. **Проверка логов:**
   - Открыть DevTools
   - Проверить консоль на наличие логов `[YouDovezu]`

### 🔗 **Тестирование в Telegram**

1. **Настройка кнопки меню** через BotFather
2. **Открытие WebApp** через кнопку в боте
3. **Проверка авторизации** - должна работать автоматически
4. **Проверка UI** - должен показывать "✅ Telegram WebApp"

## Развертывание

### 🚀 **Процесс развертывания**

1. **Сборка для продакшена:**
   ```bash
   npm run build:production
   ```

2. **Загрузка на сервер:**
   - Загрузить содержимое папки `build/`
   - Настроить веб-сервер для статических файлов

3. **Настройка BotFather:**
   - Обновить URL WebApp
   - Проверить работу кнопки меню

### 🔧 **Настройка BotFather**

1. Открыть [@BotFather](https://t.me/BotFather)
2. Выполнить команду `/setmenubutton`
3. Выбрать бота
4. Указать текст: `🚗 YouDovezu`
5. Указать URL: `https://app.youdovezu.com`

## Устранение неполадок

### ❌ **Частые проблемы**

**1. "Hash not found in initData"**
- Проверить, что WebApp открыт через Telegram
- Убедиться, что скрипт Telegram загружен

**2. "Invalid initData hash"**
- Проверить токен бота на сервере
- Убедиться, что initData не поврежден

**3. "Connection refused"**
- Проверить доступность API сервера
- Убедиться в правильности URL

### 🔧 **Решения**

**Для разработки:**
```bash
# Проверить переменные окружения
echo $REACT_APP_API_URL
echo $REACT_APP_TEST_MODE

# Перезапустить с очисткой кэша
npm start -- --reset-cache
```

**Для продакшена:**
```bash
# Проверить сборку
npm run build:production

# Проверить логи сервера
docker compose logs youdovezu.api
```

## Заключение

Интеграция с Telegram WebApp обеспечивает:

- ✅ **Безопасную авторизацию** через initData
- ✅ **Нативный пользовательский опыт** в Telegram
- ✅ **Гибкое тестирование** в браузере
- ✅ **Простое развертывание** и настройку
- ✅ **Подробное логирование** для отладки

Приложение готово к использованию как в Telegram, так и в браузере для разработки и тестирования.
