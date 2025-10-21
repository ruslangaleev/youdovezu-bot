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
  const [currentView, setCurrentView] = useState<'main' | 'search' | 'offer' | 'create-trip'>('main');
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
              <p className="placeholder-text">
                Форма создания поездки будет добавлена позже
              </p>
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