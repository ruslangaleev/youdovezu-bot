// Конфигурация для Яндекс.Карт
export const YANDEX_CONFIG = {
  // Замените на ваш реальный API ключ от Яндекс.Карт
  API_KEY: 'YOUR_YANDEX_MAPS_API_KEY',
  
  // Границы для поиска (Башкортостан)
  BOUNDS: {
    southWest: [53.0, 54.0],
    northEast: [56.0, 60.0]
  },
  
  // Настройки автодополнения
  SUGGEST_OPTIONS: {
    boundedBy: [[53.0, 54.0], [56.0, 60.0]],
    strictBounds: false,
    results: 5
  }
};

// Функция для получения API ключа
export const getYandexApiKey = (): string => {
  // В продакшене можно получать ключ из переменных окружения
  return process.env.REACT_APP_YANDEX_API_KEY || YANDEX_CONFIG.API_KEY;
};
