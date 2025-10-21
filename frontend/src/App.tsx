import React, { useState, useEffect } from 'react';
import axios from 'axios';
import './App.css';
import { apiConfig, getInitData, log, initTelegramWebApp } from './config';
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
      };
    };
    ymaps?: {
      ready: (callback: () => void) => void;
      geocode: (query: string, options?: any) => Promise<any>;
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
  const [isTestMode, setIsTestMode] = useState(false);
  const [yandexMapsInitialized, setYandexMapsInitialized] = useState(false);
  const [fromAddress, setFromAddress] = useState('');
  const [toAddress, setToAddress] = useState('');

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
      
      // Обновляем состояние при ручном вводе
      if (inputId === 'from-address') {
        setFromAddress(value);
      } else if (inputId === 'to-address') {
        setToAddress(value);
      }
      
      // Очищаем предыдущий таймер
      if (timeoutId) {
        clearTimeout(timeoutId);
      }
      
      if (value.length > 2 && window.ymaps) {
        // Добавляем задержку 300мс перед отправкой запроса
        timeoutId = setTimeout(() => {
          console.log('Отправляем запрос к Яндекс.Картам через geocode...');
          
          try {
            if (window.ymaps) {
              window.ymaps.geocode(value, {
                boundedBy: [
                  [51.0, 53.0], // юго-запад Башкортостана
                  [56.5, 60.0]  // северо-восток Башкортостана
                ],
                strictBounds: false,
                results: 5
              }).then((result: any) => {
              console.log('Результат от Яндекс.Карт:', result);
              
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
                            div.textContent = addressLine;
                            div.onclick = () => {
                              console.log('Выбран адрес:', addressLine);
                              input.value = addressLine;
                              
                              // Сохраняем адрес в состояние в зависимости от поля
                              if (inputId === 'from-address') {
                                setFromAddress(addressLine);
                              } else if (inputId === 'to-address') {
                                setToAddress(addressLine);
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
    setCurrentView('search');
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

  const enableTestMode = () => {
    setIsTestMode(true);
    setError(null);
    setLoading(false);
    
    // Создаем тестовые данные пользователя
    setUserInfo({
      isRegistered: true,
      isPrivacyConsentGiven: true,
      isPhoneConfirmed: true,
      user: {
        firstName: 'Тестовый',
        lastName: 'Пользователь',
        username: 'test_user'
      },
      capabilities: {
        canSearchTrips: true,
        canCreateTrips: true
      },
      message: 'Тестовый режим активирован'
    });
    
    console.log('Тестовый режим активирован');
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
            {!isTestMode && (
              <button onClick={enableTestMode} className="btn test-mode-btn">
                🧪 Тестовый режим
              </button>
            )}
          </div>
          {isTestMode && (
            <div className="test-mode-notice">
              <p>⚠️ Тестовый режим активен - данные не сохраняются</p>
            </div>
          )}
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
            <div className="trips-list">
              <div className="empty-state">
                <div className="empty-icon">🚗</div>
                <h3>У вас пока нет поездок</h3>
                <p>Создайте первую поездку, чтобы найти попутчиков</p>
              </div>
            </div>
            
            <div className="trips-actions">
              <button 
                className="btn create-trip-btn"
                onClick={handleCreateNewTrip}
              >
                ➕ Новая поездка
              </button>
            </div>
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
                <label>Откуда:</label>
                <div className="address-input-container">
                  <input 
                    type="text" 
                    placeholder="Например: Уфа, ул. Ленина, 1" 
                    className="address-input"
                    id="from-address"
                    value={fromAddress}
                    onChange={(e) => setFromAddress(e.target.value)}
                    onFocus={(e) => {
                      // Смещаем курсор в конец при фокусе
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
                {fromAddress && (
                  <button 
                    className="show-on-map-btn"
                    onClick={() => showOnMap(fromAddress)}
                    title="Показать на Яндекс.Картах"
                  >
                    🗺️ Показать на карте
                  </button>
                )}
              </div>
              
              <div className="form-group">
                <label>Куда:</label>
                <div className="address-input-container">
                  <input 
                    type="text" 
                    placeholder="Например: Караидель, ул. Советская, 5" 
                    className="address-input"
                    id="to-address"
                    value={toAddress}
                    onChange={(e) => setToAddress(e.target.value)}
                    onFocus={(e) => {
                      // Смещаем курсор в конец при фокусе
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
                {toAddress && (
                  <button 
                    className="show-on-map-btn"
                    onClick={() => showOnMap(toAddress)}
                    title="Показать на Яндекс.Картах"
                  >
                    🗺️ Показать на карте
                  </button>
                )}
              </div>
              
              <div className="form-group">
                <label>Комментарий:</label>
                <textarea placeholder="Дополнительная информация о поездке..."></textarea>
              </div>
              
              <button className="btn create-trip-btn">
                🚙 Создать поездку
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