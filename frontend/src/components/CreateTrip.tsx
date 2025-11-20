import React, { useEffect } from 'react';
import TelegramWebAppInfo from './TelegramWebAppInfo';

interface CreateTripProps {
  isTelegramWebApp: boolean;
  fromSettlement: string;
  toSettlement: string;
  fromAddress: string;
  toAddress: string;
  fromAddressSelected: boolean;
  toAddressSelected: boolean;
  fromFullAddress: string;
  toFullAddress: string;
  comment: string;
  creatingTrip: boolean;
  setFromSettlement: (settlement: string) => void;
  setToSettlement: (settlement: string) => void;
  setFromAddress: (address: string) => void;
  setToAddress: (address: string) => void;
  setComment: (comment: string) => void;
  handleSubmitCreateTrip: () => Promise<void>;
  clearAddress: (field: 'from' | 'to') => void;
  showOnMap: (address: string) => void;
  onBack: () => void;
}

export const CreateTrip: React.FC<CreateTripProps> = ({
  isTelegramWebApp,
  fromSettlement,
  toSettlement,
  fromAddress,
  toAddress,
  fromAddressSelected,
  toAddressSelected,
  fromFullAddress,
  toFullAddress,
  comment,
  creatingTrip,
  setFromSettlement,
  setToSettlement,
  setFromAddress,
  setToAddress,
  setComment,
  handleSubmitCreateTrip,
  clearAddress,
  showOnMap,
  onBack,
}) => {
  // Управление Main Button от Telegram
  useEffect(() => {
    if (!isTelegramWebApp || !window.Telegram?.WebApp) {
      return;
    }

    const tg = window.Telegram.WebApp;
    const mainButton = tg.MainButton;

    // Проверяем валидность формы
    const isFormValid = 
      fromSettlement.trim() !== '' &&
      toSettlement.trim() !== '' &&
      fromAddress.trim() !== '' &&
      toAddress.trim() !== '' &&
      fromAddressSelected &&
      toAddressSelected;

    if (isFormValid && !creatingTrip) {
      mainButton.setText('Создать поездку');
      mainButton.onClick(handleSubmitCreateTrip);
      mainButton.show();
    } else {
      mainButton.hide();
    }

    // Очистка при размонтировании
    return () => {
      mainButton.hide();
      mainButton.offClick(handleSubmitCreateTrip);
    };
  }, [fromSettlement, toSettlement, fromAddress, toAddress, fromAddressSelected, toAddressSelected, creatingTrip, isTelegramWebApp, handleSubmitCreateTrip]);

  // Функция для прокрутки к полю с учетом клавиатуры
  const scrollToInput = (inputElement: HTMLInputElement) => {
    // Сохраняем начальную высоту viewport
    const initialViewportHeight = window.visualViewport?.height || window.innerHeight;
    
    // Функция для выполнения прокрутки
    const performScroll = () => {
      inputElement.scrollIntoView({ behavior: 'smooth', block: 'start' });
    };
    
    // Прокручиваем сразу (на случай если клавиатура уже активна)
    performScroll();
    
    // Проверяем изменение размера viewport (появление клавиатуры)
    const checkViewportChange = () => {
      const currentViewportHeight = window.visualViewport?.height || window.innerHeight;
      // Если высота уменьшилась, значит появилась клавиатура
      if (currentViewportHeight < initialViewportHeight) {
        // Прокручиваем еще раз после появления клавиатуры
        setTimeout(() => {
          performScroll();
        }, 100);
      }
    };
    
    // Используем visualViewport API если доступен
    if (window.visualViewport) {
      window.visualViewport.addEventListener('resize', checkViewportChange, { once: true });
    } else {
      // Fallback: используем событие resize с задержкой
      const resizeHandler = () => {
        setTimeout(() => {
          checkViewportChange();
          window.removeEventListener('resize', resizeHandler);
        }, 300);
      };
      window.addEventListener('resize', resizeHandler);
    }
    
    // Также прокручиваем с задержкой на случай если клавиатура появится позже
    setTimeout(() => {
      performScroll();
    }, 300);
    
    setTimeout(() => {
      performScroll();
    }, 600);
  };
  return (
    <div className="app">
      <TelegramWebAppInfo isTelegramWebApp={isTelegramWebApp} />
      <div className="page-container">
        <div className="page-header">
          <button onClick={onBack} className="back-btn">
            ← Назад
          </button>
          <h1>Новая поездка</h1>
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
                    // Прокручиваем к полю с учетом клавиатуры
                    scrollToInput(e.target);
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
                  Показать на карте
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
                    // Прокручиваем к полю с учетом клавиатуры
                    scrollToInput(e.target);
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
                  Показать на карте
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
            
            {!isTelegramWebApp && (
              <button 
                className="btn create-trip-btn"
                onClick={handleSubmitCreateTrip}
                disabled={creatingTrip}
              >
                {creatingTrip ? (
                  <>
                    Создание...
                  </>
                ) : (
                  'Создать поездку'
                )}
              </button>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

