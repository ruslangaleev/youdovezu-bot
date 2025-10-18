import React, { useState, useEffect } from 'react';
import axios from 'axios';
import './App.css';
import { apiConfig, getInitData, log, initTelegramWebApp } from './config';
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
  }
}

/**
 * Главный компонент веб-приложения YouDovezu для Telegram
 */
function App() {
  const [userInfo, setUserInfo] = useState<any>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [currentView, setCurrentView] = useState<'main' | 'search' | 'offer'>('main');
  const [isTelegramWebApp, setIsTelegramWebApp] = useState(false);

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
  }, []);

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

      // Показываем initData в алерте для отладки
      //alert(`initData для отправки на сервер:\n\n${initData}`);

      // Отправляем POST запрос с initData
      const formData = new FormData();
      formData.append('initData', initData);

      const response = await axios.post(apiConfig.getUserInfo(), formData, apiConfig.axiosConfig);
      
      log('User info received:', response.data);
      
      // Показываем ответ сервера в алерте для отладки
      alert(`Ответ сервера:\n\nСтатус: ${response.status}\nДанные: ${JSON.stringify(response.data, null, 2)}`);
      
      setUserInfo(response.data);
    } catch (err: any) {
      log('Error checking user registration:', err);
      
      // Показываем ошибку в алерте для отладки
      alert(`Ошибка запроса:\n\nСтатус: ${err.response?.status || 'Нет ответа'}\nСообщение: ${err.message}\nДанные: ${JSON.stringify(err.response?.data || {}, null, 2)}`);
      
      if (err.response?.status === 401) {
        setError('Ошибка авторизации. Пожалуйста, откройте приложение через Telegram бота.');
      } else if (err.response?.status === 400) {
        setError('Неверные данные авторизации. Пожалуйста, обновите страницу.');
      } else {
        setError('Ошибка при проверке статуса регистрации. Попробуйте позже.');
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
          <button onClick={checkUserRegistration} className="btn">
            Попробовать снова
          </button>
        </div>
      </div>
    );
  }

  // Пользователь не зарегистрирован или не завершил регистрацию
  if (!userInfo.isRegistered) {
    return (
      <div className="app">
        <TelegramWebAppInfo isTelegramWebApp={isTelegramWebApp} />
        <div className="registration-required">
          <div className="icon">🚗</div>
          <h1>YouDovezu</h1>
          <h2>Завершите регистрацию</h2>
          <p>{userInfo.message}</p>
          <div className="steps">
            <div className="step">
              <span className="step-number">1</span>
              <span>Откройте бота @YoudovezuBot</span>
            </div>
            <div className="step">
              <span className="step-number">2</span>
              <span>Выполните команду /start</span>
            </div>
            <div className="step">
              <span className="step-number">3</span>
              <span>Согласитесь с политикой конфиденциальности</span>
            </div>
            <div className="step">
              <span className="step-number">4</span>
              <span>Подтвердите номер телефона</span>
            </div>
          </div>
          <button onClick={() => window.Telegram?.WebApp?.close()} className="btn">
            Закрыть
          </button>
        </div>
      </div>
    );
  }

  // Страница поиска поездок
  if (currentView === 'search') {
    return (
      <div className="app">
        <TelegramWebAppInfo isTelegramWebApp={isTelegramWebApp} />
        <div className="page-container">
          <div className="page-header">
            <button onClick={handleBackToMain} className="back-btn">
              ← Назад
            </button>
            <h1>🔍 Ищу машину</h1>
          </div>
          
          <div className="search-content">
            <div className="search-form">
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
                <label>Количество пассажиров:</label>
                <select>
                  <option value="1">1 пассажир</option>
                  <option value="2">2 пассажира</option>
                  <option value="3">3 пассажира</option>
                  <option value="4">4 пассажира</option>
                </select>
              </div>
              
              <button className="btn search-btn">
                🔍 Найти поездки
              </button>
            </div>
            
            <div className="search-results">
              <p className="placeholder-text">
                Введите параметры поиска для просмотра доступных поездок
              </p>
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