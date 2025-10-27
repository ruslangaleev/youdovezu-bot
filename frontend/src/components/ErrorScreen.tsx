import React from 'react';

interface ErrorScreenProps {
  error: string;
  onRetry: () => void;
}

export const ErrorScreen: React.FC<ErrorScreenProps> = ({ error, onRetry }) => {
  return (
    <div className="app">
      <div className="error">
        <h2>❌ Ошибка</h2>
        <p>{error}</p>
        <div className="error-actions">
          <button onClick={onRetry} className="btn">
            Попробовать снова
          </button>
        </div>
      </div>
    </div>
  );
};

