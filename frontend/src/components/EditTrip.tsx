import React from 'react';
import TelegramWebAppInfo from './TelegramWebAppInfo';

interface EditTripProps {
  isTelegramWebApp: boolean;
  tripId: number;
  fromSettlement: string;
  toSettlement: string;
  fromAddress: string;
  toAddress: string;
  fromAddressSelected: boolean;
  toAddressSelected: boolean;
  fromFullAddress: string;
  toFullAddress: string;
  comment: string;
  updatingTrip: boolean;
  setFromSettlement: (settlement: string) => void;
  setToSettlement: (settlement: string) => void;
  setFromAddress: (address: string) => void;
  setToAddress: (address: string) => void;
  setComment: (comment: string) => void;
  handleSubmitUpdateTrip: () => Promise<void>;
  clearAddress: (field: 'from' | 'to') => void;
  showOnMap: (address: string) => void;
  onBack: () => void;
}

export const EditTrip: React.FC<EditTripProps> = ({
  isTelegramWebApp,
  tripId,
  fromSettlement,
  toSettlement,
  fromAddress,
  toAddress,
  fromAddressSelected,
  toAddressSelected,
  fromFullAddress,
  toFullAddress,
  comment,
  updatingTrip,
  setFromSettlement,
  setToSettlement,
  setFromAddress,
  setToAddress,
  setComment,
  handleSubmitUpdateTrip,
  clearAddress,
  showOnMap,
  onBack,
}) => {
  return (
    <div className="app">
      <TelegramWebAppInfo isTelegramWebApp={isTelegramWebApp} />
      <div className="page-container">
        <div className="page-header">
          <button onClick={onBack} className="back-btn">
            ← Назад
          </button>
          <h1>✏️ Редактировать поездку</h1>
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
                  🗺️ Показать на карте
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
                  🗺️ Показать на карте
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
            
            <button 
              className="btn create-trip-btn"
              onClick={handleSubmitUpdateTrip}
              disabled={updatingTrip}
            >
              {updatingTrip ? (
                <>
                  🔄 Сохранение...
                </>
              ) : (
                '💾 Сохранить изменения'
              )}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

