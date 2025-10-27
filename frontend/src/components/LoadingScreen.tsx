import React from 'react';

interface LoadingScreenProps {
  message?: string;
}

export const LoadingScreen: React.FC<LoadingScreenProps> = ({ message = 'Загрузка...' }) => {
  return (
    <div className="app">
      <div className="loading">
        <div className="spinner"></div>
        <p>{message}</p>
      </div>
    </div>
  );
};

