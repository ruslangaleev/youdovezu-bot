import React, { useState, useEffect } from 'react';
import axios from 'axios';
import './App.css';
import { apiConfig, getInitData, log, initTelegramWebApp, config } from './config';
import { getYandexApiKey, YANDEX_CONFIG } from './yandex-config';
import TelegramWebAppInfo from './components/TelegramWebAppInfo';

// Типы для Telegram WebApp
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
        requestLocation: (callback: (location: { latitude: number; longitude: number }) => void) => void;
        showAlert: (message: string) => void;
        showConfirm: (message: string, callback: (confirmed: boolean) => void) => void;
      };
    };
    ymaps?: {
      ready: (callback: () => void) => void;
      geocode: (query: string | number[], options?: any) => Promise<any>;
    };
  }
}

/**
 * Главный компонент веб-приложения YouDovezu для Telegram
 */
function App() {
  const [userInfo, setUserInfo] = useState<any>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [currentView, setCurrentView] = useState<'main' | 'search' | 'offer' | 'create-trip'>('main');
  const [isTelegramWebApp, setIsTelegramWebApp] = useState(false);
  const [yandexMapsInitialized, setYandexMapsInitialized] = useState(false);
  const [fromAddress, setFromAddress] = useState('');
  const [toAddress, setToAddress] = useState('');
  const [fromSettlement, setFromSettlement] = useState('');
  const [toSettlement, setToSettlement] = useState('');
  const [fromAddressSelected, setFromAddressSelected] = useState(false);
  const [toAddressSelected, setToAddressSelected] = useState(false);
  const [fromCoordinates, setFromCoordinates] = useState<{lat: number, lon: number} | null>(null);
  const [toCoordinates, setToCoordinates] = useState<{lat: number, lon: number} | null>(null);
  const [fromFullAddress, setFromFullAddress] = useState('');
  const [toFullAddress, setToFullAddress] = useState('');
  const [comment, setComment] = useState('');
  const [trips, setTrips] = useState<any[]>([]);
  const [loadingTrips, setLoadingTrips] = useState(false);
  const [creatingTrip, setCreatingTrip] = useState(false);

  useEffect(() => {
    // Инициализируем Telegram WebApp
    const telegramWebAppInitialized = initTelegramWebApp();
    setIsTelegramWebApp(telegramWebAppInitialized);
    
    if (telegramWebAppInitialized) {
      log('Running in Telegram WebApp environment');
    } else {
      log('Running in browser environment (development/testing)');
    }
    
    // Проверяем регистрацию пользователя
    checkUserRegistration();
    
    // Инициализируем Яндекс.Карты для автодополнения адресов (только один раз)
    if (!yandexMapsInitialized) {
      initializeYandexMaps();
      setYandexMapsInitialized(true);
    }
  }, [yandexMapsInitialized]);

  // Отдельный useEffect для инициализации автодополнения когда переходим на страницу создания поездки
  useEffect(() => {
    if (currentView === 'create-trip') {
      // Небольшая задержка чтобы DOM успел обновиться
      setTimeout(() => {
        console.log('Проверяем элементы на странице создания поездки...');
        const fromInput = document.getElementById('from-address');
        const toInput = document.getElementById('to-address');
        console.log('from-address найден:', !!fromInput);
        console.log('to-address найден:', !!toInput);
        
        if (window.ymaps && fromInput && toInput) {
          console.log('Переинициализируем автодополнение...');
          setupAddressAutocomplete('from-address', 'from-suggestions');
          setupAddressAutocomplete('to-address', 'to-suggestions');
        }
      }, 100);
    }
    
    // Загружаем поездки при переходе на страницу списка
    // Теперь загрузка происходит напрямую в handleSearchTrips
    // if (currentView === 'search') {
    //   setLoadingTrips(true); // Устанавливаем загрузку сразу
    //   loadMyTrips();
    // }
  }, [currentView]);

  const initializeYandexMaps = () => {
    console.log('Инициализация Яндекс.Карт...');
    
    // Проверяем, не загружен ли уже скрипт
    if (document.querySelector('script[src*="api-maps.yandex.ru"]')) {
      console.log('Скрипт Яндекс.Карт уже загружен');
      if (window.ymaps) {
        window.ymaps.ready(() => {
          console.log('Яндекс.Карты готовы к использованию (уже загружены)');
          setupAddressAutocomplete('from-address', 'from-suggestions');
          setupAddressAutocomplete('to-address', 'to-suggestions');
        });
      }
      return;
    }
    
    // Загружаем скрипт Яндекс.Карт
    if (!window.ymaps) {
      const apiKey = getYandexApiKey();
      console.log('API ключ:', apiKey);
      
      const script = document.createElement('script');
      script.src = `https://api-maps.yandex.ru/2.1/?apikey=${apiKey}&lang=ru_RU`;
      
      script.onload = () => {
        console.log('Скрипт Яндекс.Карт загружен успешно');
        if (window.ymaps) {
          window.ymaps.ready(() => {
            console.log('Яндекс.Карты готовы к использованию');
            
            // Тестируем API через Geocoder
            if (window.ymaps) {
              window.ymaps.geocode('Уфа', {
                boundedBy: [
                  [51.0, 53.0], // юго-запад Башкортостана
                  [56.5, 60.0]  // северо-восток Башкортостана
                ],
                strictBounds: false,
                results: 5
              }).then((result: any) => {
                console.log('Тестовый запрос "Уфа" через geocode:', result);
              }).catch((error: any) => {
                console.error('Ошибка тестового запроса:', error);
              });
            }
            
            setupAddressAutocomplete('from-address', 'from-suggestions');
            setupAddressAutocomplete('to-address', 'to-suggestions');
          });
        }
      };
      
      script.onerror = () => {
        console.error('Ошибка загрузки Яндекс.Карт. Проверьте API ключ.');
      };
      
      document.head.appendChild(script);
      console.log('Скрипт Яндекс.Карт добавлен в DOM');
    } else {
      console.log('Яндекс.Карты уже загружены');
      if (window.ymaps) {
        window.ymaps.ready(() => {
          console.log('Яндекс.Карты готовы к использованию (повторно)');
          setupAddressAutocomplete('from-address', 'from-suggestions');
          setupAddressAutocomplete('to-address', 'to-suggestions');
        });
      }
    }
  };


  const setupAddressAutocomplete = (inputId: string, suggestionsId: string) => {
    console.log(`Настройка автодополнения для ${inputId}`);
    
    const input = document.getElementById(inputId) as HTMLInputElement;
    const suggestions = document.getElementById(suggestionsId);
    
    if (!input || !suggestions) {
      console.error(`Не найдены элементы: input=${!!input}, suggestions=${!!suggestions}`);
      return;
    }

    console.log(`Элементы найдены для ${inputId}`);

    let timeoutId: NodeJS.Timeout | null = null;

    // Обработчик изменения текста с debounce
    input.addEventListener('input', (e) => {
      const value = (e.target as HTMLInputElement).value;
      console.log(`Ввод в ${inputId}: "${value}"`);
      
      // Обновляем состояние при ручном вводе и сбрасываем флаги выбора
      if (inputId === 'from-address') {
        setFromAddress(value);
        setFromFullAddress('');
        setFromAddressSelected(false);
        setFromCoordinates(null);
      } else if (inputId === 'to-address') {
        setToAddress(value);
        setToFullAddress('');
        setToAddressSelected(false);
        setToCoordinates(null);
      }
      
      // Очищаем предыдущий таймер
      if (timeoutId) {
        clearTimeout(timeoutId);
      }
      
      if (value.length > 2 && window.ymaps) {
        // Добавляем задержку 300мс перед отправкой запроса
        timeoutId = setTimeout(() => {
          console.log('Отправляем запрос к Яндекс.Картам через geocode...');
          
          // Получаем населенный пункт НАПРЯМУЮ из DOM, чтобы получить актуальное значение
          const settlementInput = document.getElementById(inputId === 'from-address' ? 'from-settlement' : 'to-settlement') as HTMLSelectElement;
          const settlement = settlementInput ? settlementInput.value : '';
          
          console.log(`Населенный пункт (${inputId}):`, settlement);
          console.log(`Введенное значение:`, value);
          
          // Формируем полный адрес для поиска
          const fullAddress = settlement ? `${settlement}, ${value}` : value;
          
          console.log('Полный адрес для поиска:', fullAddress);
          console.log('=== ОТПРАВКА В ГЕОКОДЕР ===');
          console.log('Текст запроса:', fullAddress);
          
          try {
            if (window.ymaps) {
              window.ymaps.geocode(fullAddress, {
                boundedBy: [
                  [51.0, 53.0], // юго-запад Башкортостана
                  [56.5, 60.0]  // северо-восток Башкортостана
                ],
                strictBounds: false,
                results: 5
              }).then((result: any) => {
              console.log('Результат от Яндекс.Карт:', result);
              
              // Логируем все свойства результата
              if (result && result.geoObjects) {
                const geoObjects = result.geoObjects.toArray();
                if (geoObjects.length > 0) {
                  console.log('=== СВОЙСТВА ОБЪЕКТОВ ОТ ГЕОКОДЕРА ===');
                  geoObjects.forEach((item: any, index: number) => {
                    console.log(`\n--- Объект ${index + 1} ---`);
                    const addressLine = item.getAddressLine ? item.getAddressLine() : 'N/A';
                    const coordinates = item.geometry?.getCoordinates ? item.geometry.getCoordinates() : 'N/A';
                    const name = item.properties?.get ? item.properties.get('name') : 'N/A';
                    const kind = item.properties?.get ? item.properties.get('kind') : 'N/A';
                    const text = item.properties?.get ? item.properties.get('text') : 'N/A';
                    
                    console.log('addressLine:', addressLine);
                    console.log('coordinates:', coordinates);
                    console.log('name:', name);
                    console.log('kind:', kind);
                    console.log('text:', text);
                    
                    // Логируем metaDataProperty.GeocoderMetaData.Address.Components
                    const geocoderMetaData = item.properties?.get ? item.properties.get('GeocoderMetaData') : null;
                    if (geocoderMetaData && geocoderMetaData.Address && geocoderMetaData.Address.Components) {
                      console.log('Address Components:', geocoderMetaData.Address.Components);
                    }
                    
                    console.log('Все свойства properties:', item.properties?.getAll ? item.properties.getAll() : 'N/A');
                  });
                }
              }
              
              try {
                if (result && result.geoObjects && typeof result.geoObjects.toArray === 'function') {
                  const geoObjects = result.geoObjects.toArray();
                  if (geoObjects.length > 0) {
                    suggestions.innerHTML = '';
                    let shownCount = 0;
                    
                    geoObjects.forEach((item: any) => {
                      try {
                        const addressLine = item.getAddressLine ? item.getAddressLine() : 'Неизвестный адрес';
                        
                        // Дополнительная фильтрация по Башкортостану
                        // Исключаем общие записи типа "Республика Башкортостан"
                        const isGeneralRegion = addressLine.toLowerCase() === 'республика башкортостан' ||
                                               addressLine.toLowerCase() === 'башкортостан' ||
                                               addressLine.toLowerCase().includes('республика башкортостан, россия');
                        
                        const isSpecificLocation = addressLine.toLowerCase().includes('уфа') ||
                                                   addressLine.toLowerCase().includes('караидель') ||
                                                   addressLine.toLowerCase().includes('белебей') ||
                                                   addressLine.toLowerCase().includes('белорецк') ||
                                                   addressLine.toLowerCase().includes('бижбуляк') ||
                                                   addressLine.toLowerCase().includes('благовещенск') ||
                                                   addressLine.toLowerCase().includes('давлеканово') ||
                                                   addressLine.toLowerCase().includes('дуван') ||
                                                   addressLine.toLowerCase().includes('ишимбай') ||
                                                   addressLine.toLowerCase().includes('кумертау') ||
                                                   addressLine.toLowerCase().includes('мелеуз') ||
                                                   addressLine.toLowerCase().includes('нефтекамск') ||
                                                   addressLine.toLowerCase().includes('октябрьский') ||
                                                   addressLine.toLowerCase().includes('салават') ||
                                                   addressLine.toLowerCase().includes('сибай') ||
                                                   addressLine.toLowerCase().includes('стерлитамак') ||
                                                   addressLine.toLowerCase().includes('туймазы') ||
                                                   addressLine.toLowerCase().includes('учалы') ||
                                                   addressLine.toLowerCase().includes('янаул') ||
                                                   addressLine.toLowerCase().includes('башкортостан,') ||
                                                   addressLine.toLowerCase().includes('башкортостан, россия');
                        
                        if (!isGeneralRegion && isSpecificLocation) {
                          // Дополнительная проверка: исключаем слишком короткие или общие записи
                          const hasSpecificDetails = addressLine.includes(',') || 
                                                   addressLine.includes('ул.') || 
                                                   addressLine.includes('улица') ||
                                                   addressLine.includes('проспект') ||
                                                   addressLine.includes('пр.') ||
                                                   addressLine.includes('переулок') ||
                                                   addressLine.includes('пер.') ||
                                                   addressLine.includes('микрорайон') ||
                                                   addressLine.includes('мкр.') ||
                                                   addressLine.includes('район') ||
                                                   addressLine.includes('поселок') ||
                                                   addressLine.includes('село') ||
                                                   addressLine.includes('деревня') ||
                                                   addressLine.includes('д.') ||
                                                   addressLine.includes('с.') ||
                                                   addressLine.includes('п.') ||
                                                   addressLine.length > 20; // Длинные названия обычно содержат детали
                          
                          if (hasSpecificDetails) {
                            const div = document.createElement('div');
                            div.className = 'suggestion-item';
                            
                            // Получаем координаты объекта
                            const coordinates = item.geometry.getCoordinates();
                            const lat = coordinates[0];
                            const lon = coordinates[1];
                            
                            // Получаем свойства от геокодера
                            const name = item.properties?.get ? item.properties.get('name') : addressLine;
                            const text = item.properties?.get ? item.properties.get('text') : '';
                            
                            // Отображаем name как заголовок (выделенный, сверху)
                            const addressDiv = document.createElement('div');
                            addressDiv.className = 'suggestion-address';
                            addressDiv.textContent = name;
                            div.appendChild(addressDiv);
                            
                            // Отображаем text как описание (менее выделенное, внизу)
                            if (text) {
                              const textDiv = document.createElement('div');
                              textDiv.className = 'suggestion-full-address';
                              textDiv.textContent = text;
                              div.appendChild(textDiv);
                            }
                            
                            div.onclick = () => {
                              console.log('=== ВЫБОР АДРЕСА ===');
                              console.log('Выбран адрес:', addressLine);
                              console.log('Координаты:', { lat, lon });
                              
                              // Получаем name и text
                              const name = item.properties?.get ? item.properties.get('name') : addressLine;
                              const text = item.properties?.get ? item.properties.get('text') : '';
                              
                              // Сохраняем информацию
                              const geoObjectInfo = {
                                name: name,
                                text: text,
                                addressLine: addressLine,
                                latitude: lat,
                                longitude: lon,
                                coordinates: [lat, lon]
                              };
                              console.log('=== ИНФОРМАЦИЯ ОТ ГЕОКОДЕРА (для фронта) ===');
                              console.log(JSON.stringify(geoObjectInfo, null, 2));
                              
                              input.value = name;
                              
                              // Сохраняем адрес в состояние в зависимости от поля и устанавливаем флаги
                              if (inputId === 'from-address') {
                                setFromAddress(name);
                                setFromFullAddress(text || addressLine); // Сохраняем полный адрес для открытия на карте
                                setFromAddressSelected(true);
                                setFromCoordinates({ lat, lon });
                              } else if (inputId === 'to-address') {
                                setToAddress(name);
                                setToFullAddress(text || addressLine); // Сохраняем полный адрес для открытия на карте
                                setToAddressSelected(true);
                                setToCoordinates({ lat, lon });
                              }
                              
                              suggestions.innerHTML = '';
                              suggestions.style.display = 'none';
                            };
                            suggestions.appendChild(div);
                            shownCount++;
                          }
                        }
                      } catch (itemError) {
                        console.error('Ошибка обработки элемента:', itemError);
                      }
                    });
                    
                    if (shownCount > 0) {
                      suggestions.style.display = 'block';
                      console.log(`Показано ${shownCount} подсказок из Башкортостана`);
                    } else {
                      suggestions.style.display = 'none';
                      console.log('Подсказки из Башкортостана не найдены');
                    }
                  } else {
                    suggestions.style.display = 'none';
                    console.log('Подсказки не найдены');
                  }
                } else {
                  suggestions.style.display = 'none';
                  console.log('Некорректный формат результата');
                }
              } catch (processingError) {
                console.error('Ошибка обработки результата:', processingError);
                suggestions.style.display = 'none';
              }
              }).catch((error: any) => {
                console.error('Ошибка получения подсказок:', error);
                suggestions.style.display = 'none';
              });
            }
          } catch (geocodeError) {
            console.error('Ошибка вызова geocode:', geocodeError);
            suggestions.style.display = 'none';
          }
        }, 300);
      } else {
        suggestions.style.display = 'none';
        if (value.length <= 2) {
          console.log('Слишком короткий запрос');
        }
        if (!window.ymaps) {
          console.log('Яндекс.Карты не загружены');
        }
      }
    });

    // Скрываем подсказки при клике вне поля
    document.addEventListener('click', (e) => {
      if (!input.contains(e.target as Node) && !suggestions.contains(e.target as Node)) {
        suggestions.style.display = 'none';
      }
    });

    // Скрываем подсказки при потере фокуса
    input.addEventListener('blur', () => {
      setTimeout(() => {
        suggestions.style.display = 'none';
      }, 200);
    });
    
    console.log(`Автодополнение настроено для ${inputId}`);
  };

  const checkUserRegistration = async () => {
    try {
      setLoading(true);
      setError(null);

      // Получаем initData от Telegram WebApp
      const initData = getInitData();

      if (!initData) {
        setError('Не удалось получить данные авторизации от Telegram. Пожалуйста, откройте приложение через Telegram бота.');
        return;
      }

      log('Checking user registration with initData length:', initData.length);

      // Отправляем POST запрос с initData
      const formData = new FormData();
      formData.append('initData', initData);

      const response = await axios.post(apiConfig.getUserInfo(), formData, apiConfig.axiosConfig);
      
      log('User info received:', response.data);
      
      // Отладочная информация
      console.log('=== DEBUG: Server Response ===');
      console.log('Full response.data:', response.data);
      console.log('isPrivacyConsentGiven:', response.data.isPrivacyConsentGiven);
      console.log('isPhoneConfirmed:', response.data.isPhoneConfirmed);
      console.log('isRegistered:', response.data.isRegistered);
      console.log('==============================');
      
      setUserInfo(response.data);
    } catch (err: any) {
      log('Error checking user registration:', err);
      
      // Обрабатываем различные типы ошибок
      if (err.response?.status === 401) {
        setError('Ошибка авторизации. Пожалуйста, откройте приложение через Telegram бота.');
      } else if (err.response?.status === 400) {
        const errorMessage = err.response?.data?.error || 'Неверные данные авторизации';
        setError(`${errorMessage}. Пожалуйста, обновите страницу.`);
      } else if (err.response?.status === 500) {
        const errorMessage = err.response?.data?.error || 'Внутренняя ошибка сервера';
        setError(`${errorMessage}. Попробуйте позже или обратитесь в поддержку.`);
      } else if (err.code === 'NETWORK_ERROR' || !err.response) {
        setError('Ошибка сети. Проверьте подключение к интернету и попробуйте снова.');
      } else {
        setError('Неожиданная ошибка. Попробуйте позже или обратитесь в поддержку.');
      }
    } finally {
      setLoading(false);
    }
  };

  const handleSearchTrips = () => {
    setLoadingTrips(true); // Устанавливаем загрузку сразу при переходе
    setCurrentView('search');
    loadMyTrips(); // Сразу начинаем загрузку данных
  };

  const handleOfferTrip = () => {
    setCurrentView('offer');
  };

  const handleBackToMain = () => {
    setCurrentView('main');
  };

  const handleCreateNewTrip = () => {
    setCurrentView('create-trip');
  };

  const handleSubmitCreateTrip = async () => {
    try {
      setCreatingTrip(true);
      
      // Проверяем обязательные поля
      if (!fromSettlement || !toSettlement) {
        alert('Пожалуйста, выберите населенные пункты отправления и назначения');
        setCreatingTrip(false);
        return;
      }

      if (!fromAddress || !toAddress) {
        alert('Пожалуйста, укажите адреса отправления и назначения');
        setCreatingTrip(false);
        return;
      }

      // Получаем initData
      const initData = getInitData();
      if (!initData) {
        alert('Не удалось получить данные авторизации');
        setCreatingTrip(false);
        return;
      }

      // Подготавливаем данные для отправки
      const tripData = {
        fromAddress: fromAddress,
        fromSettlement: fromSettlement,
        fromLatitude: fromCoordinates?.lat,
        fromLongitude: fromCoordinates?.lon,
        toAddress: toAddress,
        toSettlement: toSettlement,
        toLatitude: toCoordinates?.lat,
        toLongitude: toCoordinates?.lon,
        comment: comment
      };

      console.log('Отправка поездки на сервер:', tripData);

      // Отправляем запрос на сервер
      const response = await axios.post(
        `${config.apiBaseUrl}/api/webapp/trips?initData=${encodeURIComponent(initData)}`, 
        tripData, 
        {
          headers: {
            'Content-Type': 'application/json'
          }
        }
      );

      console.log('Поездка создана:', response.data);

      // Показываем успешное сообщение
      if (isTelegramWebApp && window.Telegram?.WebApp?.showAlert) {
        window.Telegram.WebApp.showAlert('Поездка успешно создана!');
      } else {
        alert('Поездка успешно создана!');
      }

      // Очищаем форму
      setFromAddress('');
      setToAddress('');
      setFromSettlement('');
      setToSettlement('');
      setComment('');
      setFromAddressSelected(false);
      setToAddressSelected(false);
      setFromCoordinates(null);
      setToCoordinates(null);
      setFromFullAddress('');
      setToFullAddress('');

      // Возвращаемся на страницу со списком поездок
      setLoadingTrips(true); // Показываем загрузку при переходе
      setCurrentView('search');
      loadMyTrips(); // Обновляем список поездок
    } catch (error: any) {
      console.error('Ошибка при создании поездки:', error);
      
      const errorMessage = error.response?.data?.error || 'Не удалось создать поездку';
      if (isTelegramWebApp && window.Telegram?.WebApp?.showAlert) {
        window.Telegram.WebApp.showAlert(errorMessage);
      } else {
        alert(errorMessage);
      }
    } finally {
      setCreatingTrip(false);
    }
  };

  const loadMyTrips = async () => {
    try {
      // Получаем initData
      const initData = getInitData();
      if (!initData) {
        console.error('Не удалось получить данные авторизации');
        setLoadingTrips(false);
        return;
      }

      console.log('Загрузка списка поездок...');
      
      // Добавляем минимальную задержку чтобы спиннер был виден минимум 500мс
      const minDelay = new Promise(resolve => setTimeout(resolve, 500));

      // Отправляем запрос на сервер и дожидаемся минимум 500мс
      const [response] = await Promise.all([
        axios.post(
          `${config.apiBaseUrl}/api/webapp/trips/my?initData=${encodeURIComponent(initData)}`,
          {},
          {
            headers: {
              'Content-Type': 'application/json'
            }
          }
        ),
        minDelay
      ]);

      console.log('Список поездок получен:', response.data);
      
      // Сохраняем список поездок
      if (response.data.trips) {
        setTrips(response.data.trips);
      }
    } catch (error: any) {
      console.error('Ошибка при загрузке поездок:', error);
      
      // Не показываем ошибку пользователю, просто очищаем список
      setTrips([]);
    } finally {
      setLoadingTrips(false);
    }
  };

  const showOnMap = (address: string) => {
    if (!address.trim()) return;
    
    // Кодируем адрес для URL
    const encodedAddress = encodeURIComponent(address);
    
    // Открываем Яндекс.Карты с поиском адреса
    const yandexMapsUrl = `https://yandex.ru/maps/?text=${encodedAddress}`;
    window.open(yandexMapsUrl, '_blank');
  };

  const clearAddress = (field: 'from' | 'to') => {
    if (field === 'from') {
      setFromAddress('');
      setFromFullAddress('');
      setFromAddressSelected(false);
      setFromCoordinates(null);
      const input = document.getElementById('from-address') as HTMLInputElement;
      if (input) {
        input.value = '';
        input.focus();
        // Смещаем курсор в конец
        setTimeout(() => {
          const length = input.value.length;
          input.setSelectionRange(length, length);
        }, 0);
      }
    } else if (field === 'to') {
      setToAddress('');
      setToFullAddress('');
      setToAddressSelected(false);
      setToCoordinates(null);
      const input = document.getElementById('to-address') as HTMLInputElement;
      if (input) {
        input.value = '';
        input.focus();
        // Смещаем курсор в конец
        setTimeout(() => {
          const length = input.value.length;
          input.setSelectionRange(length, length);
        }, 0);
      }
    }
  };

  // Функция для запроса геолокации через Telegram WebApp
  const requestCurrentLocation = (field: 'from' | 'to') => {
    if (!isTelegramWebApp || !window.Telegram?.WebApp) {
      alert('Геолокация доступна только в Telegram WebApp');
      return;
    }

    try {
      window.Telegram.WebApp.requestLocation((location) => {
        console.log('Получена геолокация:', location);
        
        // Конвертируем координаты в адрес через Яндекс.Карты
        convertCoordinatesToAddress(location.latitude, location.longitude, field);
      });
    } catch (error) {
      console.error('Ошибка при запросе геолокации:', error);
      alert('Не удалось получить местоположение');
    }
  };

  // Функция для конвертации координат в адрес
  const convertCoordinatesToAddress = async (lat: number, lon: number, field: 'from' | 'to') => {
    if (!window.ymaps) {
      alert('Яндекс.Карты не загружены');
      return;
    }

    try {
      const result = await window.ymaps.geocode([lat, lon], {
        results: 1
      });

      if (result.geoObjects.getLength() > 0) {
        const geoObject = result.geoObjects.get(0);
        const address = geoObject.getAddressLine();
        
        console.log('Адрес по координатам:', address);
        
        // Устанавливаем адрес в соответствующее поле
        if (field === 'from') {
          setFromAddress(address);
          const input = document.getElementById('from-address') as HTMLInputElement;
          if (input) {
            input.value = address;
          }
        } else if (field === 'to') {
          setToAddress(address);
          const input = document.getElementById('to-address') as HTMLInputElement;
          if (input) {
            input.value = address;
          }
        }
        
        // Показываем уведомление
        if (isTelegramWebApp && window.Telegram?.WebApp?.showAlert) {
          window.Telegram.WebApp.showAlert(`Адрес определен: ${address}`);
        } else {
          alert(`Адрес определен: ${address}`);
        }
      } else {
        alert('Не удалось определить адрес по координатам');
      }
    } catch (error) {
      console.error('Ошибка при геокодировании:', error);
      alert('Ошибка при определении адреса');
    }
  };

  if (loading) {
    return (
      <div className="app">
        <TelegramWebAppInfo isTelegramWebApp={isTelegramWebApp} />
        <div className="loading">
          <div className="spinner"></div>
          <p>Проверка статуса регистрации...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="app">
        <TelegramWebAppInfo isTelegramWebApp={isTelegramWebApp} />
        <div className="error">
          <h2>❌ Ошибка</h2>
          <p>{error}</p>
          <div className="error-actions">
          <button onClick={checkUserRegistration} className="btn">
            Попробовать снова
          </button>
          </div>
        </div>
      </div>
    );
  }

  // Пользователь не зарегистрирован или не завершил регистрацию
  if (!userInfo.isRegistered || !userInfo.isPrivacyConsentGiven || !userInfo.isPhoneConfirmed) {
    // Определяем текущий этап регистрации (только 2 шага)
    const getRegistrationSteps = () => {
      const steps = [];
      
      // Отладочная информация
      console.log('Debug - userInfo:', userInfo);
      console.log('Debug - isPrivacyConsentGiven:', userInfo.isPrivacyConsentGiven);
      console.log('Debug - isPhoneConfirmed:', userInfo.isPhoneConfirmed);
      
      // Этап 1: Согласие с политикой конфиденциальности
      const privacyStep = {
        number: 1,
        text: "Согласитесь с политикой конфиденциальности",
        completed: userInfo.isPrivacyConsentGiven || false
      };
      steps.push(privacyStep);
      
      // Этап 2: Подтверждение номера телефона
      const phoneStep = {
        number: 2,
        text: "Подтвердите номер телефона",
        completed: userInfo.isPhoneConfirmed || false
      };
      steps.push(phoneStep);
      
      return steps;
    };

    const steps = getRegistrationSteps();
    const completedSteps = steps.filter(step => step.completed).length;
    const totalSteps = steps.length;

    return (
      <div className="app">
        <TelegramWebAppInfo isTelegramWebApp={isTelegramWebApp} />
        <div className="registration-required">
          <div className="icon">🚗</div>
          <h1>YouDovezu</h1>
          <h2>Завершите регистрацию</h2>
          <p>{userInfo.message}</p>
          
          {/* Прогресс регистрации */}
          <div className="registration-progress">
            <div className="progress-bar">
              <div 
                className="progress-fill" 
                style={{ width: `${(completedSteps / totalSteps) * 100}%` }}
              ></div>
            </div>
            <p className="progress-text">
              Прогресс: {completedSteps} из {totalSteps} шагов
            </p>
            </div>
          
          <div className="steps">
            {steps.map((step, index) => (
              <div key={index} className={`step ${step.completed ? 'completed' : 'pending'}`}>
                <span className={`step-number ${step.completed ? 'completed' : 'pending'}`}>
                  {step.completed ? '✓' : step.number}
                </span>
                <span className={step.completed ? 'completed-text' : 'pending-text'}>
                  {step.text}
                </span>
            </div>
            ))}
          </div>
          
          <button onClick={() => window.Telegram?.WebApp?.close()} className="btn">
            Закрыть
          </button>
        </div>
      </div>
    );
  }

  // Страница поиска поездок (теперь показывает список поездок пользователя)
  if (currentView === 'search') {
    return (
      <div className="app">
        <TelegramWebAppInfo isTelegramWebApp={isTelegramWebApp} />
        <div className="page-container">
          <div className="page-header">
            <button onClick={handleBackToMain} className="back-btn">
              ← Назад
            </button>
            <h1>🔍 Мои поездки</h1>
          </div>
          
          <div className="trips-content">
            {loadingTrips ? (
              <div className="loading">
                <div className="spinner"></div>
                <p>Загрузка поездок...</p>
              </div>
            ) : trips.length === 0 ? (
              <div className="trips-list">
                <div className="empty-state">
                  <div className="empty-icon">🚗</div>
                  <h3>У вас пока нет поездок</h3>
                  <p>Создайте первую поездку, чтобы найти попутчиков</p>
                </div>
              </div>
            ) : (
              <div className="trips-list">
                {trips.map((trip) => (
                  <div key={trip.id} className="trip-item">
                    <div className="trip-route">
                      <div className="trip-from">
                        <span className="trip-label">Откуда:</span>
                        <span className="trip-address">{trip.fromAddress}</span>
                        <span className="trip-settlement">{trip.fromSettlement}</span>
                      </div>
                      <div className="trip-arrow">→</div>
                      <div className="trip-to">
                        <span className="trip-label">Куда:</span>
                        <span className="trip-address">{trip.toAddress}</span>
                        <span className="trip-settlement">{trip.toSettlement}</span>
                      </div>
                    </div>
                    {trip.comment && (
                      <div className="trip-comment">
                        <span className="trip-label">Комментарий:</span>
                        <span>{trip.comment}</span>
                      </div>
                    )}
                    <div className="trip-info">
                      <span className="trip-date">
                        Создано: {new Date(trip.createdAt).toLocaleDateString('ru-RU')}
                      </span>
                      <span className={`trip-status trip-status-${trip.status.toLowerCase()}`}>
                        {trip.status === 'Active' ? 'Активна' : 'Закрыта'}
                      </span>
                    </div>
              </div>
                ))}
              </div>
            )}
            
            {!loadingTrips && (
              <div className="trips-actions">
                <button 
                  className="btn create-trip-btn"
                  onClick={handleCreateNewTrip}
                >
                  ➕ Новая поездка
                </button>
            </div>
            )}
          </div>
        </div>
      </div>
    );
  }

  // Страница предложения поездки
  if (currentView === 'offer') {
    return (
      <div className="app">
        <TelegramWebAppInfo isTelegramWebApp={isTelegramWebApp} />
        <div className="page-container">
          <div className="page-header">
            <button onClick={handleBackToMain} className="back-btn">
              ← Назад
            </button>
            <h1>🚙 Предложить машину</h1>
          </div>
          
          <div className="offer-content">
            <div className="offer-form">
              <div className="form-group">
                <label>Откуда:</label>
                <input type="text" placeholder="Например: Караидель" />
              </div>
              
              <div className="form-group">
                <label>Куда:</label>
                <input type="text" placeholder="Например: Уфа" />
              </div>
              
              <div className="form-group">
                <label>Дата поездки:</label>
                <input type="date" />
              </div>
              
              <div className="form-group">
                <label>Время отправления:</label>
                <input type="time" />
              </div>
              
              <div className="form-group">
                <label>Цена за место:</label>
                <input type="number" placeholder="500" />
                <span className="currency">₽</span>
              </div>
              
              <div className="form-group">
                <label>Количество свободных мест:</label>
                <select>
                  <option value="1">1 место</option>
                  <option value="2">2 места</option>
                  <option value="3">3 места</option>
                  <option value="4">4 места</option>
                </select>
              </div>
              
              <div className="form-group">
                <label>Комментарий:</label>
                <textarea placeholder="Дополнительная информация о поездке..."></textarea>
              </div>
              
              <button className="btn offer-btn">
                🚙 Создать поездку
              </button>
            </div>
          </div>
        </div>
      </div>
    );
  }

  // Страница создания новой поездки
  if (currentView === 'create-trip') {
    return (
      <div className="app">
        <TelegramWebAppInfo isTelegramWebApp={isTelegramWebApp} />
        <div className="page-container">
          <div className="page-header">
            <button onClick={() => setCurrentView('search')} className="back-btn">
              ← Назад
            </button>
            <h1>➕ Новая поездка</h1>
          </div>
          
          <div className="create-trip-content">
            <div className="create-trip-form">
              <div className="form-group">
                <label>Населенный пункт (Откуда):</label>
                <select 
                  className="address-input"
                  id="from-settlement"
                  value={fromSettlement}
                  onChange={(e) => setFromSettlement(e.target.value)}
                >
                  <option value="">Выберите населенный пункт</option>
                  <option value="Караидель">Караидель</option>
                </select>
              </div>

              <div className="form-group">
                <label>Адрес (Откуда):</label>
                <div className="address-input-container">
                  <input 
                    type="text" 
                    placeholder={fromSettlement ? "Например: ул. Ленина, 1" : "Сначала выберите населенный пункт"}
                    className="address-input"
                    id="from-address"
                    value={fromAddress}
                    onChange={(e) => setFromAddress(e.target.value)}
                    disabled={!fromSettlement}
                    onFocus={(e) => {
                      setTimeout(() => {
                        const length = e.target.value.length;
                        e.target.setSelectionRange(length, length);
                      }, 0);
                    }}
                  />
                  {fromAddress && (
                    <button 
                      className="clear-btn"
                      onClick={() => clearAddress('from')}
                      title="Очистить поле"
                    >
                      ✕
                    </button>
                  )}
                  <div className="address-suggestions" id="from-suggestions"></div>
                </div>
                {fromAddressSelected && (
                  <button 
                    className="show-on-map-btn"
                    onClick={() => showOnMap(fromFullAddress)}
                    title="Показать на Яндекс.Картах"
                  >
                    🗺️ Показать на карте
                  </button>
                )}
              </div>
              
              <div className="form-group">
                <label>Населенный пункт (Куда):</label>
                <select 
                  className="address-input"
                  id="to-settlement"
                  value={toSettlement}
                  onChange={(e) => setToSettlement(e.target.value)}
                >
                  <option value="">Выберите населенный пункт</option>
                  <option value="Караидель">Караидель</option>
                </select>
              </div>

              <div className="form-group">
                <label>Адрес (Куда):</label>
                <div className="address-input-container">
                  <input 
                    type="text" 
                    placeholder={toSettlement ? "Например: ул. Советская, 5" : "Сначала выберите населенный пункт"}
                    className="address-input"
                    id="to-address"
                    value={toAddress}
                    onChange={(e) => setToAddress(e.target.value)}
                    disabled={!toSettlement}
                    onFocus={(e) => {
                      setTimeout(() => {
                        const length = e.target.value.length;
                        e.target.setSelectionRange(length, length);
                      }, 0);
                    }}
                  />
                  {toAddress && (
                    <button 
                      className="clear-btn"
                      onClick={() => clearAddress('to')}
                      title="Очистить поле"
                    >
                      ✕
                    </button>
                  )}
                  <div className="address-suggestions" id="to-suggestions"></div>
                </div>
                {toAddressSelected && (
                  <button 
                    className="show-on-map-btn"
                    onClick={() => showOnMap(toFullAddress)}
                    title="Показать на Яндекс.Картах"
                  >
                    🗺️ Показать на карте
                  </button>
                )}
              </div>
              
              <div className="form-group">
                <label>Комментарий:</label>
                <textarea 
                  placeholder="Дополнительная информация о поездке..."
                  value={comment}
                  onChange={(e) => setComment(e.target.value)}
                ></textarea>
              </div>
              
              <button 
                className="btn create-trip-btn"
                onClick={handleSubmitCreateTrip}
                disabled={creatingTrip}
              >
                {creatingTrip ? (
                  <>
                    🔄 Создание...
                  </>
                ) : (
                  '🚙 Создать поездку'
                )}
              </button>
            </div>
          </div>
        </div>
      </div>
    );
  }

  // Главное меню - показывается только полностью зарегистрированным пользователям
  return (
    <div className="app">
      <TelegramWebAppInfo isTelegramWebApp={isTelegramWebApp} />
      <div className="main-menu">
        <div className="header">
          <div className="icon">🚗</div>
          <h1>YouDovezu</h1>
          <p>Добро пожаловать, {userInfo.user.firstName}!</p>
        </div>

        <div className="menu-buttons">
          {userInfo.capabilities.canSearchTrips && (
            <button 
              className="menu-btn search-btn"
              onClick={handleSearchTrips}
            >
              <span className="btn-icon">🔍</span>
              <span className="btn-text">Ищу машину</span>
              <span className="btn-subtitle">Найти поездку</span>
            </button>
          )}

          {userInfo.capabilities.canCreateTrips && (
            <button 
              className="menu-btn offer-btn"
              onClick={handleOfferTrip}
            >
              <span className="btn-icon">🚙</span>
              <span className="btn-text">Предложить машину</span>
              <span className="btn-subtitle">Создать поездку</span>
            </button>
          )}

          {!userInfo.capabilities.canSearchTrips && !userInfo.capabilities.canCreateTrips && (
            <div className="no-capabilities">
              <p>У вас пока нет доступа к функциям приложения.</p>
              <p>Обратитесь к администратору для получения прав.</p>
            </div>
          )}
        </div>

        <div className="user-info">
          <p className="user-status">
            {userInfo.capabilities.canSearchTrips && '👤 Пассажир'}
            {userInfo.capabilities.canSearchTrips && userInfo.capabilities.canCreateTrips && ' • '}
            {userInfo.capabilities.canCreateTrips && '🚗 Водитель'}
            {userInfo.user.isTrialActive && ' (Триал)'}
          </p>
        </div>
      </div>
    </div>
  );
}

export default App;