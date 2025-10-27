import React from 'react';
import { TelegramWebAppInfo } from './TelegramWebAppInfo';

interface MainMenuProps {
  userInfo: any;
  isTelegramWebApp: boolean;
  onSearchTrips: () => void;
  onOfferTrip: () => void;
}

export const MainMenu: React.FC<MainMenuProps> = ({ 
  userInfo, 
  isTelegramWebApp,
  onSearchTrips,
  onOfferTrip
}) => {
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
              onClick={onSearchTrips}
            >
              <span className="btn-icon">🔍</span>
              <span className="btn-text">Ищу машину</span>
              <span className="btn-subtitle">Найти поездку</span>
            </button>
          )}

          {userInfo.capabilities.canCreateTrips && (
            <button 
              className="menu-btn offer-btn"
              onClick={onOfferTrip}
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
};
