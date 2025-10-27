// Конфигурация для веб-приложения YouDovezu
export const config = {
  // URL API сервера
  apiBaseUrl: process.env.REACT_APP_API_URL || 'http://localhost:8080',
  
  // URL веб-приложения (для Telegram WebApp)
  webAppUrl: process.env.REACT_APP_WEBAPP_URL || 'http://localhost:3000',
  
  // Настройки для разных окружений
  environment: process.env.NODE_ENV || 'development',
  
  // Таймауты запросов
  requestTimeout: 10000, // 10 секунд
  
  // Настройки логирования
  enableLogging: process.env.NODE_ENV === 'development',
  
  // Настройки для тестирования
  testMode: process.env.REACT_APP_TEST_MODE === 'true',
  
  // Флаг использования тестового initData (если true, принудительно использует тестовый initData)
  useTestInitData: process.env.REACT_APP_USE_TEST_INIT_DATA === 'true',
  
  // Заглушка initData для тестирования (только для разработки)
  testInitData: process.env.REACT_APP_TEST_INIT_DATA || '',
  
  // Настройки Telegram WebApp
  telegramWebApp: {
    // Включить Telegram WebApp SDK
    enableSDK: true,
    // Показывать кнопку закрытия
    showCloseButton: true,
    // Развернуть на весь экран
    expand: true,
    // Показывать главную кнопку
    showMainButton: false
  }
};

// Логирование конфигурации при загрузке
console.log('[Config] Loaded configuration:', {
  apiBaseUrl: config.apiBaseUrl,
  testMode: config.testMode,
  useTestInitData: config.useTestInitData,
  hasTestInitData: !!config.testInitData,
  testInitDataLength: config.testInitData?.length || 0
});

// Вспомогательные функции для работы с API
export const apiConfig = {
  // Полный URL для API endpoints
  getUserInfo: () => `${config.apiBaseUrl}/api/webapp/user`,
  getRegistrationStatus: () => `${config.apiBaseUrl}/api/webapp/registration-status`,
  
  // Настройки для axios
  axiosConfig: {
    timeout: config.requestTimeout,
    headers: {
      'Content-Type': 'application/x-www-form-urlencoded',
    }
  }
};

// Функция для логирования (только в development)
export const log = (...args: any[]) => {
  if (config.enableLogging) {
    console.log('[YouDovezu]', ...args);
  }
};

// Функция для получения initData
export const getInitData = (): string => {
  // Логируем конфигурацию для отладки
  console.log('[getInitData] Config:', {
    useTestInitData: config.useTestInitData,
    testInitDataLength: config.testInitData?.length || 0,
    hasTelegramWebApp: !!window.Telegram?.WebApp?.initData
  });
  
  // Если включен флаг принудительного использования тестового initData
  if (config.useTestInitData && config.testInitData) {
    console.log('[getInitData] Using test initData from environment variables (forced)');
    return config.testInitData;
  }
  
  // В реальном Telegram WebApp
  if (window.Telegram?.WebApp?.initData) {
    console.log('[getInitData] Using initData from Telegram WebApp');
    return window.Telegram.WebApp.initData;
  }
  
  // Для тестирования - используем параметр URL
  const urlParams = new URLSearchParams(window.location.search);
  const urlInitData = urlParams.get('initData');
  
  if (urlInitData) {
    console.log('[getInitData] Using initData from URL parameter');
    return urlInitData;
  }
  
  // Используем переменную окружения для тестирования (fallback)
  if (config.testInitData) {
    console.log('[getInitData] Using test initData from environment variables (fallback)');
    return config.testInitData;
  }
  
  console.warn('[getInitData] No initData available!');
  return '';
};

// Функция для инициализации Telegram WebApp
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
    
    log('Telegram WebApp initialized:', {
      platform: tg.platform,
      version: tg.version,
      colorScheme: tg.colorScheme,
      themeParams: tg.themeParams
    });
    
    return true;
  }
  
  return false;
};

export default config;
