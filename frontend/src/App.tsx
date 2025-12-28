import React, { useState, useEffect, useCallback } from 'react';
import axios from 'axios';
import './App.css';
import { apiConfig, getInitData, log, initTelegramWebApp, config } from './config';
import { getYandexApiKey } from './yandex-config';
import TelegramWebAppInfo from './components/TelegramWebAppInfo';
import { CreateTrip } from './components/CreateTrip';
import { EditTrip } from './components/EditTrip';
import { UploadDocuments } from './components/UploadDocuments';
import { DocumentVerification } from './components/DocumentVerification';
import { ModerationList } from './components/ModerationList';
import { ModerationDetail } from './components/ModerationDetail';

// Типы для Telegram WebApp
interface TelegramThemeParams {
  bg_color?: string;
  text_color?: string;
  hint_color?: string;
  link_color?: string;
  button_color?: string;
  button_text_color?: string;
  secondary_bg_color?: string;
}

interface TelegramMainButton {
  text: string;
  color: string;
  textColor: string;
  isVisible: boolean;
  isActive: boolean;
  isProgressVisible: boolean;
  setText: (text: string) => void;
  onClick: (callback: () => void) => void;
  offClick: (callback: () => void) => void;
  show: () => void;
  hide: () => void;
  enable: () => void;
  disable: () => void;
  showProgress: (leaveActive?: boolean) => void;
  hideProgress: () => void;
  setParams: (params: { text?: string; color?: string; text_color?: string; is_active?: boolean; is_visible?: boolean }) => void;
}

declare global {
  interface Window {
    Telegram?: {
      WebApp?: {
        close: () => void;
        initData: string;
        initDataUnsafe: any;
        expand: () => void;
        enableClosingConfirmation: () => void;
        disableVerticalSwipes: () => void;
        ready: () => void;
        platform: string;
        version: string;
        colorScheme: string;
        themeParams: TelegramThemeParams;
        MainButton: TelegramMainButton;
        onEvent: (event: string, callback: () => void) => void;
        offEvent: (event: string, callback: () => void) => void;
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
  const [currentView, setCurrentView] = useState<'main' | 'search' | 'offer' | 'create-trip' | 'edit-trip' | 'upload-documents' | 'document-verification' | 'moderation-list' | 'moderation-detail' | 'trip-search' | 'trip-details'>('main');
  const [isAdmin, setIsAdmin] = useState(false);
  const [selectedDocumentsId, setSelectedDocumentsId] = useState<number | null>(null);
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
  const [editingTripId, setEditingTripId] = useState<number | null>(null);
  const [updatingTrip, setUpdatingTrip] = useState(false);
  const [deletingTripId, setDeletingTripId] = useState<number | null>(null);
  const [settlements, setSettlements] = useState<Array<{id: number, name: string, type: string, tripsCount: number}>>([]);
  const [loadingSettlements, setLoadingSettlements] = useState(false);
  const [selectedSettlement, setSelectedSettlement] = useState<string | null>(null);
  const [tripOffers, setTripOffers] = useState<any[]>([]);
  const [loadingTripOffers, setLoadingTripOffers] = useState(false);
  const [selectedTrip, setSelectedTrip] = useState<any | null>(null);
  const [tripDetails, setTripDetails] = useState<any | null>(null);
  const [loadingTripDetails, setLoadingTripDetails] = useState(false);
  const [offerPrice, setOfferPrice] = useState<string>('');
  const [offerComment, setOfferComment] = useState<string>('');
  const [submittingOffer, setSubmittingOffer] = useState(false);
  const [existingOffer, setExistingOffer] = useState<any | null>(null);

  // Функция для настройки автодополнения адресов (должна быть объявлена до initializeYandexMaps)
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
                    
                    // Создаем элемент списка
                    const li = document.createElement('li');
                    li.textContent = addressLine;
                    li.style.cursor = 'pointer';
                    li.style.padding = '8px';
                    li.style.borderBottom = '1px solid #eee';
                    
                    li.addEventListener('click', () => {
                      console.log('Выбран адрес:', addressLine);
                      console.log('Координаты:', coordinates);
                      
                      // Устанавливаем выбранный адрес
                      if (inputId === 'from-address') {
                        setFromAddress(addressLine);
                        setFromFullAddress(addressLine);
                        setFromAddressSelected(true);
                        setFromCoordinates(coordinates);
                      } else if (inputId === 'to-address') {
                        setToAddress(addressLine);
                        setToFullAddress(addressLine);
                        setToAddressSelected(true);
                        setToCoordinates(coordinates);
                      }
                      
                      // Очищаем список предложений
                      suggestions.innerHTML = '';
                      
                      // Устанавливаем значение в поле ввода
                      input.value = addressLine;
                      
                      // Устанавливаем курсор в конец
                      const length = addressLine.length;
                      setTimeout(() => {
                        input.focus();
                        input.setSelectionRange(length, length);
                      }, 0);
                    });
                    
                    suggestions.appendChild(li);
                  });
                } else {
                  console.log('Нет результатов от геокодера');
                  suggestions.innerHTML = '<li style="padding: 8px; color: #999;">Адрес не найден</li>';
                }
              } else {
                console.log('Результат не содержит geoObjects');
                suggestions.innerHTML = '<li style="padding: 8px; color: #999;">Адрес не найден</li>';
              }
            }).catch((error: any) => {
              console.error('Ошибка при запросе к Яндекс.Картам:', error);
              suggestions.innerHTML = '<li style="padding: 8px; color: #999;">Ошибка при поиске адреса</li>';
            });
          }
          }, 300);
        }
      }
    });
  };

  // Функция для инициализации Яндекс.Карт (должна быть объявлена до useEffect, который её использует)
  const initializeYandexMaps = useCallback(() => {
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
  }, []);

  useEffect(() => {
    // Инициализируем Telegram WebApp
    const telegramWebAppInitialized = initTelegramWebApp();
    setIsTelegramWebApp(telegramWebAppInitialized);
    
    if (telegramWebAppInitialized) {
      log('Running in Telegram WebApp environment');
    } else {
      log('Running in browser environment (development/testing)');
    }
    
    // Устанавливаем цвета темы Telegram
    if (window.Telegram?.WebApp) {
      const tg = window.Telegram.WebApp;
      const themeParams = tg.themeParams;
      
      // Устанавливаем CSS-переменные для цветов темы
      if (themeParams) {
        const root = document.documentElement;
        
        // Основные цвета
        if (themeParams.bg_color) {
          root.style.setProperty('--tg-theme-bg-color', themeParams.bg_color);
          root.style.setProperty('--app-bg-color', themeParams.bg_color);
        }
        if (themeParams.text_color) {
          root.style.setProperty('--tg-theme-text-color', themeParams.text_color);
        }
        if (themeParams.hint_color) {
          root.style.setProperty('--tg-theme-hint-color', themeParams.hint_color);
        }
        if (themeParams.link_color) {
          root.style.setProperty('--tg-theme-link-color', themeParams.link_color);
        }
        if (themeParams.button_color) {
          root.style.setProperty('--tg-theme-button-color', themeParams.button_color);
        }
        if (themeParams.button_text_color) {
          root.style.setProperty('--tg-theme-button-text-color', themeParams.button_text_color);
        }
        if (themeParams.secondary_bg_color) {
          root.style.setProperty('--tg-theme-secondary-bg-color', themeParams.secondary_bg_color);
        }
      }
      
      // Обработчик изменения темы
      tg.onEvent('themeChanged', () => {
        const updatedThemeParams = tg.themeParams;
        if (updatedThemeParams) {
          const root = document.documentElement;
          if (updatedThemeParams.bg_color) {
            root.style.setProperty('--tg-theme-bg-color', updatedThemeParams.bg_color);
            root.style.setProperty('--app-bg-color', updatedThemeParams.bg_color);
          }
          if (updatedThemeParams.text_color) {
            root.style.setProperty('--tg-theme-text-color', updatedThemeParams.text_color);
          }
          if (updatedThemeParams.secondary_bg_color) {
            root.style.setProperty('--tg-theme-secondary-bg-color', updatedThemeParams.secondary_bg_color);
          }
        }
      });
    }
    
    // Проверяем регистрацию пользователя
    checkUserRegistration();
    
    // Проверяем права администратора
    checkAdminStatus();
    
    // Инициализируем Яндекс.Карты для автодополнения адресов (только один раз)
    if (!yandexMapsInitialized) {
      initializeYandexMaps();
      setYandexMapsInitialized(true);
    }
  }, [yandexMapsInitialized, initializeYandexMaps]);

  // Скрываем Main Button на всех страницах, кроме создания и редактирования поездки
  useEffect(() => {
    if (!isTelegramWebApp || !window.Telegram?.WebApp) {
      return;
    }

    const tg = window.Telegram.WebApp;
    const mainButton = tg.MainButton;

    // Скрываем Main Button на всех страницах, кроме создания и редактирования
    if (currentView !== 'create-trip' && currentView !== 'edit-trip') {
      mainButton.hide();
    }
  }, [currentView, isTelegramWebApp]);

  // Отдельный useEffect для инициализации автодополнения когда переходим на страницу создания/редактирования поездки
  useEffect(() => {
    if (currentView === 'create-trip' || currentView === 'edit-trip') {
      // Небольшая задержка чтобы DOM успел обновиться
      setTimeout(() => {
        console.log('Проверяем элементы на странице создания/редактирования поездки...');
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

  const checkAdminStatus = async () => {
    try {
      const initData = getInitData();
      if (!initData) {
        setIsAdmin(false);
        return;
      }

      const response = await axios.post(
        `${config.apiBaseUrl}/api/webapp/moderation/check-admin?initData=${encodeURIComponent(initData)}`
      );
      setIsAdmin(response.data.isAdmin || false);
    } catch (err: any) {
      console.error('Error checking admin status:', err);
      setIsAdmin(false);
    }
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

  const handleOfferTrip = async () => {
    // Проверяем статус документов перед открытием страницы
    try {
      const initData = getInitData();
      if (!initData) {
        throw new Error('Не удалось получить данные авторизации');
      }

      const response = await axios.post(
        `${config.apiBaseUrl}/api/webapp/driver-documents/status?initData=${encodeURIComponent(initData)}`
      );

      if (response.data.status === 'not_submitted') {
        // Документы не отправлены - показываем страницу загрузки
        setCurrentView('upload-documents');
      } else if (response.data.status === 'Pending' || response.data.status === 'UnderReview') {
        // Документы на проверке - показываем страницу статуса
        setCurrentView('document-verification');
      } else if (response.data.status === 'Approved') {
        // Документы одобрены - открываем страницу поиска объявлений (список населенных пунктов)
        setCurrentView('trip-search');
        loadSettlements(); // Загружаем список населенных пунктов
      } else {
        // Документы отклонены или другой статус - показываем страницу статуса
        setCurrentView('document-verification');
      }
    } catch (err: any) {
      // При ошибке показываем страницу загрузки документов
      console.error('Error checking documents status:', err);
      setCurrentView('upload-documents');
    }
  };

  const loadSettlements = async () => {
    try {
      setLoadingSettlements(true);
      const initData = getInitData();
      if (!initData) {
        console.error('Не удалось получить данные авторизации');
        setLoadingSettlements(false);
        return;
      }

      const response = await axios.post(
        `${config.apiBaseUrl}/api/webapp/trips/search/settlements?initData=${encodeURIComponent(initData)}`
      );

      if (response.data) {
        setSettlements(response.data);
      }
    } catch (error: any) {
      console.error('Ошибка при загрузке населенных пунктов:', error);
      alert(error.response?.data?.error || 'Не удалось загрузить список населенных пунктов');
    } finally {
      setLoadingSettlements(false);
    }
  };

  const handleSelectSettlement = async (settlementName: string) => {
    setSelectedSettlement(settlementName);
    await loadTripOffers(settlementName);
  };

  const loadTripOffers = async (settlementName: string) => {
    try {
      setLoadingTripOffers(true);
      const initData = getInitData();
      if (!initData) {
        console.error('Не удалось получить данные авторизации');
        setLoadingTripOffers(false);
        return;
      }

      const response = await axios.post(
        `${config.apiBaseUrl}/api/webapp/trips/search?initData=${encodeURIComponent(initData)}&settlement=${encodeURIComponent(settlementName)}`
      );

      if (response.data) {
        // Для каждого объявления проверяем, было ли отправлено предложение
        const tripsWithOffers = await Promise.all(
          response.data.map(async (trip: any) => {
            try {
              const offerResponse = await axios.post(
                `${config.apiBaseUrl}/api/webapp/trips/offers/${trip.id}/my?initData=${encodeURIComponent(initData)}`
              );
              return {
                ...trip,
                hasOffer: offerResponse.data?.hasOffer || false,
                existingOffer: offerResponse.data?.offer || null
              };
            } catch (error: any) {
              // Если предложения нет, это нормально
              return {
                ...trip,
                hasOffer: false,
                existingOffer: null
              };
            }
          })
        );
        setTripOffers(tripsWithOffers);
      }
    } catch (error: any) {
      console.error('Ошибка при загрузке объявлений:', error);
      alert(error.response?.data?.error || 'Не удалось загрузить список объявлений');
    } finally {
      setLoadingTripOffers(false);
    }
  };

  const handleViewTripDetails = async (trip: any) => {
    setSelectedTrip(trip);
    setCurrentView('trip-details');
    await loadTripDetails(trip.id);
  };

  const loadTripDetails = async (tripId: number) => {
    try {
      setLoadingTripDetails(true);
      const initData = getInitData();
      if (!initData) {
        console.error('Не удалось получить данные авторизации');
        setLoadingTripDetails(false);
        return;
      }

      // Загружаем детальную информацию об объявлении
      const detailsResponse = await axios.post(
        `${config.apiBaseUrl}/api/webapp/trips/search/${tripId}/details?initData=${encodeURIComponent(initData)}`
      );

      if (detailsResponse.data) {
        setTripDetails(detailsResponse.data);
        setExistingOffer(detailsResponse.data.existingOffer || null);
      }

      // Проверяем, было ли уже отправлено предложение
      try {
        const offerResponse = await axios.post(
          `${config.apiBaseUrl}/api/webapp/trips/offers/${tripId}/my?initData=${encodeURIComponent(initData)}`
        );

        if (offerResponse.data?.hasOffer && offerResponse.data?.offer) {
          setExistingOffer(offerResponse.data.offer);
        }
      } catch (error: any) {
        // Если предложения нет, это нормально
        console.log('Предложение не найдено');
      }
    } catch (error: any) {
      console.error('Ошибка при загрузке деталей объявления:', error);
      alert(error.response?.data?.error || 'Не удалось загрузить детальную информацию об объявлении');
      // Возвращаемся к списку объявлений
      setCurrentView('trip-search');
    } finally {
      setLoadingTripDetails(false);
    }
  };

  const handleSubmitOffer = async () => {
    if (!selectedTrip) {
      alert('Объявление не выбрано');
      return;
    }

    // Валидация
    const price = parseFloat(offerPrice);
    if (isNaN(price) || price <= 0) {
      alert('Пожалуйста, укажите корректную цену (положительное число)');
      return;
    }

    try {
      setSubmittingOffer(true);
      const initData = getInitData();
      if (!initData) {
        alert('Не удалось получить данные авторизации');
        setSubmittingOffer(false);
        return;
      }

      const response = await axios.post(
        `${config.apiBaseUrl}/api/webapp/trips/offers/${selectedTrip.id}?initData=${encodeURIComponent(initData)}`,
        {
          price: price,
          comment: offerComment.trim() || null
        },
        {
          headers: {
            'Content-Type': 'application/json'
          }
        }
      );

      if (response.data) {
        alert('Предложение успешно отправлено!');
        setExistingOffer(response.data);
        setOfferPrice('');
        setOfferComment('');
        // Обновляем детальную информацию
        await loadTripDetails(selectedTrip.id);
      }
    } catch (error: any) {
      console.error('Ошибка при отправке предложения:', error);
      alert(error.response?.data?.error || 'Не удалось отправить предложение');
    } finally {
      setSubmittingOffer(false);
    }
  };

  const handleBackToMain = () => {
    setCurrentView('main');
  };

  const handleCreateNewTrip = () => {
    setCurrentView('create-trip');
  };

  const handleEditTrip = (trip: any) => {
    // Заполняем форму данными поездки
    setEditingTripId(trip.id);
    setFromSettlement(trip.fromSettlement || '');
    setToSettlement(trip.toSettlement || '');
    setFromAddress(trip.fromAddress || '');
    setToAddress(trip.toAddress || '');
    setComment(trip.comment || '');
    setFromAddressSelected(!!trip.fromAddress);
    setToAddressSelected(!!trip.toAddress);
    setFromFullAddress(trip.fromAddress || '');
    setToFullAddress(trip.toAddress || '');
    // Если есть координаты, устанавливаем их
    if (trip.fromLatitude && trip.fromLongitude) {
      setFromCoordinates({ lat: trip.fromLatitude, lon: trip.fromLongitude });
    }
    if (trip.toLatitude && trip.toLongitude) {
      setToCoordinates({ lat: trip.toLatitude, lon: trip.toLongitude });
    }
    // Устанавливаем значения в DOM элементы для автокомплита
    setTimeout(() => {
      const fromAddressInput = document.getElementById('from-address') as HTMLInputElement;
      const toAddressInput = document.getElementById('to-address') as HTMLInputElement;
      if (fromAddressInput && trip.fromAddress) {
        fromAddressInput.value = trip.fromAddress;
      }
      if (toAddressInput && trip.toAddress) {
        toAddressInput.value = trip.toAddress;
      }
    }, 100);
    setCurrentView('edit-trip');
  };

  const handleSubmitUpdateTrip = async () => {
    try {
      if (!editingTripId) {
        alert('Не выбрана поездка для редактирования');
        return;
      }

      setUpdatingTrip(true);
      
      // Проверяем обязательные поля
      if (!fromSettlement || !toSettlement) {
        alert('Пожалуйста, выберите населенные пункты отправления и назначения');
        setUpdatingTrip(false);
        return;
      }

      if (!fromAddress || !toAddress) {
        alert('Пожалуйста, укажите адреса отправления и назначения');
        setUpdatingTrip(false);
        return;
      }

      // Получаем initData
      const initData = getInitData();
      if (!initData) {
        alert('Не удалось получить данные авторизации');
        setUpdatingTrip(false);
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

      console.log('Отправка обновления поездки на сервер:', tripData);

      // Отправляем запрос на сервер
      const response = await axios.put(
        `${config.apiBaseUrl}/api/webapp/trips/${editingTripId}?initData=${encodeURIComponent(initData)}`, 
        tripData, 
        {
          headers: {
            'Content-Type': 'application/json'
          }
        }
      );

      console.log('Поездка обновлена:', response.data);

      // Показываем успешное сообщение
      alert('Поездка успешно обновлена!');

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
      setEditingTripId(null);

      // Возвращаемся на страницу со списком поездок
      setLoadingTrips(true); // Показываем загрузку при переходе
      setCurrentView('search');
      loadMyTrips(); // Обновляем список поездок
    } catch (error: any) {
      console.error('Ошибка при обновлении поездки:', error);
      
      const errorMessage = error.response?.data?.error || 'Не удалось обновить поездку';
      alert(errorMessage);
    } finally {
      setUpdatingTrip(false);
    }
  };

  const handleDeleteTrip = async (tripId: number) => {
    // Показываем подтверждение
    const confirmMessage = 'Вы уверены, что хотите удалить эту поездку?';
    
    const confirmed = await new Promise<boolean>((resolve) => {
      if (isTelegramWebApp && window.Telegram?.WebApp?.showConfirm) {
        window.Telegram.WebApp.showConfirm(confirmMessage, (confirmed) => {
          resolve(confirmed);
        });
      } else {
        resolve(window.confirm(confirmMessage));
      }
    });

    if (!confirmed) {
      return; // Пользователь отменил удаление
    }

    try {
      setDeletingTripId(tripId);
      
      // Получаем initData
      const initData = getInitData();
      if (!initData) {
        alert('Не удалось получить данные авторизации');
        setDeletingTripId(null);
        return;
      }

      console.log('Удаление поездки:', tripId);

      // Отправляем запрос на сервер
      const response = await axios.delete(
        `${config.apiBaseUrl}/api/webapp/trips/${tripId}?initData=${encodeURIComponent(initData)}`,
        {
          headers: {
            'Content-Type': 'application/json'
          }
        }
      );

      console.log('Поездка удалена:', response.data);

      // Показываем успешное сообщение
      alert('Поездка успешно удалена!');

      // Обновляем список поездок
      setLoadingTrips(true);
      loadMyTrips();
    } catch (error: any) {
      console.error('Ошибка при удалении поездки:', error);
      
      const errorMessage = error.response?.data?.error || 'Не удалось удалить поездку';
      alert(errorMessage);
    } finally {
      setDeletingTripId(null);
    }
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
      alert('Поездка успешно создана!');

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
      alert(errorMessage);
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
        alert(`Адрес определен: ${address}`);
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
          <h2>Ошибка</h2>
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
          <div className="icon"></div>
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
            <h1>Мои поездки</h1>
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
                  <div className="empty-icon"></div>
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
                        Создано: {new Date(trip.createdAt).toLocaleString('ru-RU', {
                          year: 'numeric',
                          month: 'long',
                          day: 'numeric',
                          hour: '2-digit',
                          minute: '2-digit'
                        })}
                      </span>
                      <span className={`trip-status trip-status-${trip.status.toLowerCase()}`}>
                        {trip.status === 'Active' ? 'Активна' : 'Закрыта'}
                      </span>
                    </div>
                    <div className="trip-actions">
                      <button 
                        className="btn edit-trip-btn"
                        onClick={() => handleEditTrip(trip)}
                      >
                        Редактировать
                      </button>
                      <button 
                        className="btn delete-trip-btn"
                        onClick={() => handleDeleteTrip(trip.id)}
                        disabled={deletingTripId === trip.id}
                      >
                        {deletingTripId === trip.id ? 'Удаление...' : 'Удалить'}
              </button>
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
                  Новая поездка
                </button>
              </div>
            )}
          </div>
        </div>
      </div>
    );
  }

  // Страница поиска объявлений (список населенных пунктов)
  if (currentView === 'trip-search') {
    // Если выбран населенный пункт, показываем список объявлений
    if (selectedSettlement) {
      return (
        <div className="app">
          <TelegramWebAppInfo isTelegramWebApp={isTelegramWebApp} />
          <div className="page-container">
            <div className="page-header">
              <button onClick={() => {
                setSelectedSettlement(null);
                setTripOffers([]);
              }} className="back-btn">
                ← Назад
              </button>
              <h1>Объявления в {selectedSettlement}</h1>
              </div>
              
            <div className="trips-content">
              {loadingTripOffers ? (
                <div className="loading">
                  <div className="spinner"></div>
                  <p>Загрузка объявлений...</p>
              </div>
              ) : tripOffers.length === 0 ? (
                <div className="empty-state">
                  <div className="empty-icon"></div>
                  <h3>В этом населенном пункте пока нет объявлений</h3>
                  <p>Попробуйте выбрать другой населенный пункт</p>
                </div>
              ) : (
                <div className="trips-list">
                  {tripOffers.map((trip) => (
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
                          Создано: {new Date(trip.createdAt).toLocaleString('ru-RU', {
                            year: 'numeric',
                            month: 'long',
                            day: 'numeric',
                            hour: '2-digit',
                            minute: '2-digit'
                          })}
                        </span>
                      </div>
                      <div className="trip-actions">
                        {trip.hasOffer ? (
                          <button 
                            className="btn offer-sent-btn"
                            onClick={() => handleViewTripDetails(trip)}
                            disabled
                          >
                            Предложение отправлено
                          </button>
                        ) : (
                          <button 
                            className="btn offer-price-btn"
                            onClick={() => handleViewTripDetails(trip)}
                          >
                            Предложить цену
                          </button>
                        )}
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </div>
        </div>
      );
    }

    // Показываем список населенных пунктов
    return (
      <div className="app">
        <TelegramWebAppInfo isTelegramWebApp={isTelegramWebApp} />
        <div className="page-container">
          <div className="page-header">
            <button onClick={handleBackToMain} className="back-btn">
              ← Назад
              </button>
            <h1>Поиск объявлений</h1>
            </div>
            
          <div className="settlements-content">
            {loadingSettlements ? (
              <div className="loading">
                <div className="spinner"></div>
                <p>Загрузка населенных пунктов...</p>
              </div>
            ) : settlements.length === 0 ? (
              <div className="empty-state">
                <div className="empty-icon"></div>
                <h3>Нет доступных населенных пунктов</h3>
                <p>Пока нет объявлений от Requester'ов</p>
              </div>
            ) : (
              <div className="settlements-list">
                {settlements.map((settlement) => (
                  <div 
                    key={settlement.id} 
                    className="settlement-item"
                    onClick={() => handleSelectSettlement(settlement.name)}
                  >
                    <div className="settlement-info">
                      <h3>{settlement.name}</h3>
                      <span className="settlement-type">{settlement.type}</span>
                    </div>
                    <div className="settlement-trips-count">
                      <span className="trips-count">{settlement.tripsCount}</span>
                      <span className="trips-label">
                        {settlement.tripsCount === 1 ? 'объявление' : 
                         settlement.tripsCount < 5 ? 'объявления' : 'объявлений'}
                      </span>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>
      </div>
    );
  }

  // Страница детальной информации об объявлении и предложения цены
  if (currentView === 'trip-details' && selectedTrip) {
    return (
      <div className="app">
        <TelegramWebAppInfo isTelegramWebApp={isTelegramWebApp} />
        <div className="page-container">
          <div className="page-header">
            <button onClick={() => {
              setSelectedTrip(null);
              setTripDetails(null);
              setExistingOffer(null);
              setOfferPrice('');
              setOfferComment('');
              setCurrentView('trip-search');
            }} className="back-btn">
              ← Назад
            </button>
            <h1>Объявление</h1>
          </div>
          
          <div className="trip-details-content">
            {loadingTripDetails ? (
              <div className="loading">
                <div className="spinner"></div>
                <p>Загрузка деталей объявления...</p>
              </div>
            ) : tripDetails ? (
              <>
                {/* Детальная информация об объявлении */}
                <div className="trip-details-info">
                  <div className="trip-detail-section">
                    <h3>Откуда</h3>
                    <p className="trip-detail-address">{tripDetails.trip.fromAddress}</p>
                    <p className="trip-detail-settlement">{tripDetails.trip.fromSettlement}</p>
                    {tripDetails.trip.fromLatitude && tripDetails.trip.fromLongitude && (
                      <button 
                        className="btn show-map-btn"
                        onClick={() => showOnMap(tripDetails.trip.fromAddress)}
                      >
                        Показать на карте
                      </button>
                    )}
                  </div>

                  <div className="trip-detail-section">
                    <h3>🎯 Куда</h3>
                    <p className="trip-detail-address">{tripDetails.trip.toAddress}</p>
                    <p className="trip-detail-settlement">{tripDetails.trip.toSettlement}</p>
                    {tripDetails.trip.toLatitude && tripDetails.trip.toLongitude && (
                      <button 
                        className="btn show-map-btn"
                        onClick={() => showOnMap(tripDetails.trip.toAddress)}
                      >
                        Показать на карте
                      </button>
                    )}
                  </div>

                  {tripDetails.trip.comment && (
                    <div className="trip-detail-section">
                      <h3>Комментарий Requester'а</h3>
                      <p className="trip-detail-comment">{tripDetails.trip.comment}</p>
                    </div>
                  )}

                  <div className="trip-detail-section">
                    <h3>Requester</h3>
                    <p className="trip-detail-requester">
                      {tripDetails.requester.firstName} {tripDetails.requester.lastName}
                      {tripDetails.requester.username && ` (@${tripDetails.requester.username})`}
              </p>
            </div>

                  <div className="trip-detail-section">
                    <h3>Дата создания</h3>
                    <p className="trip-detail-date">
                      {new Date(tripDetails.trip.createdAt).toLocaleString('ru-RU', {
                        year: 'numeric',
                        month: 'long',
                        day: 'numeric',
                        hour: '2-digit',
                        minute: '2-digit'
                      })}
                    </p>
                  </div>
                </div>

                {/* Форма предложения цены */}
                {existingOffer ? (
                  <div className="offer-status-section">
                    <div className="offer-status-message success">
                      <h3>Предложение отправлено</h3>
                      <p>Ваша цена: <strong>{existingOffer.price} ₽</strong></p>
                      {existingOffer.comment && (
                        <p>Ваш комментарий: {existingOffer.comment}</p>
                      )}
                      <p className="offer-status">
                        Статус: {existingOffer.status === 'Pending' ? 'Ожидает ответа' : 
                                 existingOffer.status === 'Accepted' ? 'Принято' : 
                                 existingOffer.status === 'Rejected' ? 'Отклонено' : existingOffer.status}
                      </p>
                      <p className="offer-date">
                        Отправлено: {new Date(existingOffer.createdAt).toLocaleString('ru-RU', {
                          year: 'numeric',
                          month: 'long',
                          day: 'numeric',
                          hour: '2-digit',
                          minute: '2-digit'
                        })}
                      </p>
                    </div>
                  </div>
                ) : (
                  <div className="offer-form-section">
                    <h2>Предложить цену</h2>
                    <div className="offer-form">
                      <div className="form-group">
                        <label htmlFor="offer-price">Цена за поездку (₽) *</label>
                        <input
                          id="offer-price"
                          type="number"
                          min="1"
                          step="0.01"
                          value={offerPrice}
                          onChange={(e) => setOfferPrice(e.target.value)}
                          placeholder="Например: 500"
                          disabled={submittingOffer}
                        />
                      </div>

                      <div className="form-group">
                        <label htmlFor="offer-comment">Комментарий к предложению (необязательно)</label>
                        <textarea
                          id="offer-comment"
                          value={offerComment}
                          onChange={(e) => setOfferComment(e.target.value)}
                          placeholder="Дополнительная информация о вашем предложении..."
                          rows={4}
                          disabled={submittingOffer}
                        />
                      </div>

                      <button
                        className="btn submit-offer-btn"
                        onClick={handleSubmitOffer}
                        disabled={submittingOffer || !offerPrice || parseFloat(offerPrice) <= 0}
                      >
                        {submittingOffer ? 'Отправка...' : 'Отправить предложение'}
                      </button>
                    </div>
                  </div>
                )}
              </>
            ) : (
              <div className="empty-state">
                <div className="empty-icon"></div>
                <h3>Не удалось загрузить детальную информацию</h3>
              </div>
            )}
          </div>
        </div>
      </div>
    );
  }

  // Страница предложения поездки (старая, не используется)
  if (currentView === 'offer') {
    return (
      <div className="app">
        <TelegramWebAppInfo isTelegramWebApp={isTelegramWebApp} />
        <div className="page-container">
          <div className="page-header">
            <button onClick={handleBackToMain} className="back-btn">
              ← Назад
            </button>
            <h1>Предложить машину</h1>
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
                Создать поездку
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
      <CreateTrip
        isTelegramWebApp={isTelegramWebApp}
        fromSettlement={fromSettlement}
        toSettlement={toSettlement}
        fromAddress={fromAddress}
        toAddress={toAddress}
        fromAddressSelected={fromAddressSelected}
        toAddressSelected={toAddressSelected}
        fromFullAddress={fromFullAddress}
        toFullAddress={toFullAddress}
        comment={comment}
        creatingTrip={creatingTrip}
        setFromSettlement={setFromSettlement}
        setToSettlement={setToSettlement}
        setFromAddress={setFromAddress}
        setToAddress={setToAddress}
        setComment={setComment}
        handleSubmitCreateTrip={handleSubmitCreateTrip}
        clearAddress={clearAddress}
        showOnMap={showOnMap}
        onBack={() => setCurrentView('search')}
      />
    );
  }

  // Страница редактирования поездки
  if (currentView === 'edit-trip' && editingTripId) {
    return (
      <EditTrip
        isTelegramWebApp={isTelegramWebApp}
        tripId={editingTripId}
        fromSettlement={fromSettlement}
        toSettlement={toSettlement}
        fromAddress={fromAddress}
        toAddress={toAddress}
        fromAddressSelected={fromAddressSelected}
        toAddressSelected={toAddressSelected}
        fromFullAddress={fromFullAddress}
        toFullAddress={toFullAddress}
        comment={comment}
        updatingTrip={updatingTrip}
        setFromSettlement={setFromSettlement}
        setToSettlement={setToSettlement}
        setFromAddress={setFromAddress}
        setToAddress={setToAddress}
        setComment={setComment}
        handleSubmitUpdateTrip={handleSubmitUpdateTrip}
        clearAddress={clearAddress}
        showOnMap={showOnMap}
        onBack={() => setCurrentView('search')}
      />
    );
  }

  // Страница загрузки документов
  if (currentView === 'upload-documents') {
    return (
      <UploadDocuments
        isTelegramWebApp={isTelegramWebApp}
        onBack={handleBackToMain}
        onSubmitted={() => setCurrentView('document-verification')}
      />
    );
  }

  // Страница проверки документов
  if (currentView === 'document-verification') {
    return (
      <DocumentVerification
        isTelegramWebApp={isTelegramWebApp}
        onBack={handleBackToMain}
        onUploadAgain={() => setCurrentView('upload-documents')}
      />
    );
  }

  // Страница списка документов на модерации
  if (currentView === 'moderation-list') {
    return (
      <ModerationList
        isTelegramWebApp={isTelegramWebApp}
        onBack={handleBackToMain}
        onSelectDocument={(documentsId) => {
          setSelectedDocumentsId(documentsId);
          setCurrentView('moderation-detail');
        }}
      />
    );
  }

  // Страница детальной проверки документа
  if (currentView === 'moderation-detail' && selectedDocumentsId !== null) {
    return (
      <ModerationDetail
        isTelegramWebApp={isTelegramWebApp}
        documentsId={selectedDocumentsId}
        onBack={() => {
          setSelectedDocumentsId(null);
          setCurrentView('moderation-list');
        }}
        onApproved={() => {
          setSelectedDocumentsId(null);
          setCurrentView('moderation-list');
        }}
        onRejected={() => {
          setSelectedDocumentsId(null);
          setCurrentView('moderation-list');
        }}
      />
    );
  }

  // Главное меню - показывается только полностью зарегистрированным пользователям
  return (
    <div className="app">
      <TelegramWebAppInfo isTelegramWebApp={isTelegramWebApp} />
      <div className="main-menu">
        <div className="header">
          <div className="icon"></div>
          <h1>YouDovezu</h1>
          <p>Добро пожаловать, {userInfo.user.firstName}!</p>
        </div>

        <div className="menu-buttons">
          {userInfo.capabilities.canSearchTrips && (
            <button 
              className="menu-btn search-btn"
              onClick={handleSearchTrips}
            >
              <span className="btn-icon"></span>
              <span className="btn-text">Ищу машину</span>
            </button>
          )}

            <button 
              className="menu-btn offer-btn"
              onClick={handleOfferTrip}
            >
              <span className="btn-icon"></span>
              <span className="btn-text">Предложить машину</span>
            </button>

          {isAdmin && (
            <button 
              className="menu-btn moderation-btn"
              onClick={() => setCurrentView('moderation-list')}
            >
              <span className="btn-icon"></span>
              <span className="btn-text">Модерация</span>
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
            {userInfo.capabilities.canSearchTrips && 'Пассажир'}
            {userInfo.capabilities.canSearchTrips && userInfo.capabilities.canCreateTrips && ' • '}
            {userInfo.capabilities.canCreateTrips && 'Водитель'}
            {userInfo.user.isTrialActive && ' (Триал)'}
          </p>
        </div>
      </div>
    </div>
  );
}

export default App;