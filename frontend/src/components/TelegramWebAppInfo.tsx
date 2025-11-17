import React from 'react';

interface TelegramWebAppInfoProps {
  isTelegramWebApp: boolean;
}

/**
 * Компонент для отображения информации о Telegram WebApp
 * Показывается только в режиме разработки (development)
 */
export const TelegramWebAppInfo: React.FC<TelegramWebAppInfoProps> = ({ isTelegramWebApp }) => {
  // Скрываем компонент в production
  if (process.env.NODE_ENV === 'production') {
    return null;
  }

  if (!isTelegramWebApp) {
    return (
      <div style={{
        position: 'fixed',
        top: '10px',
        right: '10px',
        background: '#ff6b6b',
        color: 'white',
        padding: '8px 12px',
        borderRadius: '6px',
        fontSize: '12px',
        zIndex: 1000,
        boxShadow: '0 2px 8px rgba(0,0,0,0.2)'
      }}>
        ⚠️ Тестовый режим
      </div>
    );
  }

  return (
    <div style={{
      position: 'fixed',
      top: '10px',
      right: '10px',
      background: '#51cf66',
      color: 'white',
      padding: '8px 12px',
      borderRadius: '6px',
      fontSize: '12px',
      zIndex: 1000,
      boxShadow: '0 2px 8px rgba(0,0,0,0.2)'
    }}>
      ✅ Telegram WebApp
    </div>
  );
};

export default TelegramWebAppInfo;
