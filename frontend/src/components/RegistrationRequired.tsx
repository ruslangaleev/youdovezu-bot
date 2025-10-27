import React from 'react';

interface RegistrationRequiredProps {
  userInfo: any;
  isTelegramWebApp: boolean;
}

export const RegistrationRequired: React.FC<RegistrationRequiredProps> = ({ 
  userInfo, 
  isTelegramWebApp 
}) => {
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
};

