// Конфигурация для Яндекс.Карт
export const YANDEX_CONFIG = {
  // API ключ от Яндекс.Карт
  API_KEY: 'e65061c9-c977-4afe-bf14-80c8911922f9',
  
  // Границы для поиска (Башкортостан)
  BOUNDS: {
    southWest: [51.0, 53.0], // юго-запад
    northEast: [56.5, 60.0]  // северо-восток
  }
};

// Функция для получения API ключа
export const getYandexApiKey = (): string => {
  // В продакшене можно получать ключ из переменных окружения
  return process.env.REACT_APP_YANDEX_API_KEY || YANDEX_CONFIG.API_KEY;
};
