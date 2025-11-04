import React, { useState, useRef } from 'react';
import TelegramWebAppInfo from './TelegramWebAppInfo';
import axios from 'axios';
import { config } from '../config';
import { getInitData } from '../config';

interface UploadDocumentsProps {
  isTelegramWebApp: boolean;
  onBack: () => void;
  onSubmitted: () => void;
}

export const UploadDocuments: React.FC<UploadDocumentsProps> = ({
  isTelegramWebApp,
  onBack,
  onSubmitted,
}) => {
  const [driverLicenseFront, setDriverLicenseFront] = useState<File | null>(null);
  const [driverLicenseBack, setDriverLicenseBack] = useState<File | null>(null);
  const [vehicleRegistrationFront, setVehicleRegistrationFront] = useState<File | null>(null);
  const [vehicleRegistrationBack, setVehicleRegistrationBack] = useState<File | null>(null);
  const [avatar, setAvatar] = useState<File | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const driverLicenseFrontRef = useRef<HTMLInputElement>(null);
  const driverLicenseBackRef = useRef<HTMLInputElement>(null);
  const vehicleRegistrationFrontRef = useRef<HTMLInputElement>(null);
  const vehicleRegistrationBackRef = useRef<HTMLInputElement>(null);
  const avatarRef = useRef<HTMLInputElement>(null);

  const handleFileChange = (file: File | null, setter: (file: File | null) => void) => {
    setter(file);
    setError(null);
  };

  const handleSubmit = async () => {
    // Проверяем обязательные файлы
    if (!driverLicenseFront || !driverLicenseBack || !vehicleRegistrationFront || !vehicleRegistrationBack || !avatar) {
      setError('Пожалуйста, загрузите все необходимые документы');
      if (isTelegramWebApp && window.Telegram?.WebApp?.showAlert) {
        window.Telegram.WebApp.showAlert('Пожалуйста, загрузите все необходимые документы');
      }
      return;
    }

    setSubmitting(true);
    setError(null);

    try {
      const initData = getInitData();
      if (!initData) {
        throw new Error('Не удалось получить данные авторизации');
      }

      const formData = new FormData();
      formData.append('driverLicenseFront', driverLicenseFront);
      formData.append('driverLicenseBack', driverLicenseBack);
      formData.append('vehicleRegistrationFront', vehicleRegistrationFront);
      formData.append('vehicleRegistrationBack', vehicleRegistrationBack);
      formData.append('avatar', avatar);

      const response = await axios.post(
        `${config.apiBaseUrl}/api/webapp/driver-documents/upload?initData=${encodeURIComponent(initData)}`,
        formData,
        {
          headers: {
            'Content-Type': 'multipart/form-data',
          },
        }
      );

      if (response.data) {
        if (isTelegramWebApp && window.Telegram?.WebApp?.showAlert) {
          window.Telegram.WebApp.showAlert('Документы успешно отправлены на проверку');
        } else {
          alert('Документы успешно отправлены на проверку');
        }
        onSubmitted();
      }
    } catch (err: any) {
      const errorMessage = err.response?.data?.error || err.message || 'Ошибка при загрузке документов';
      setError(errorMessage);
      if (isTelegramWebApp && window.Telegram?.WebApp?.showAlert) {
        window.Telegram.WebApp.showAlert(errorMessage);
      } else {
        alert(errorMessage);
      }
    } finally {
      setSubmitting(false);
    }
  };

  const renderFileInput = (
    label: string,
    file: File | null,
    setter: (file: File | null) => void,
    ref: React.RefObject<HTMLInputElement | null>,
    accept: string = 'image/*'
  ) => {
    return (
      <div className="form-group">
        <label>{label}:</label>
        <div className="file-upload-container">
          <input
            type="file"
            ref={ref}
            accept={accept}
            onChange={(e) => {
              const file = e.target.files?.[0] || null;
              handleFileChange(file, setter);
            }}
            style={{ display: 'none' }}
          />
          <button
            type="button"
            className="btn file-upload-btn"
            onClick={() => ref.current?.click()}
          >
            {file ? `📎 ${file.name}` : '📁 Выбрать файл'}
          </button>
          {file && (
            <button
              type="button"
              className="btn clear-file-btn"
              onClick={() => {
                setter(null);
                if (ref.current) {
                  ref.current.value = '';
                }
              }}
            >
              ✕
            </button>
          )}
        </div>
        {file && (
          <div className="file-preview">
            <img src={URL.createObjectURL(file)} alt="Preview" />
          </div>
        )}
      </div>
    );
  };

  return (
    <div className="app">
      <TelegramWebAppInfo isTelegramWebApp={isTelegramWebApp} />
      <div className="page-container">
        <div className="page-header">
          <button onClick={onBack} className="back-btn">
            ← Назад
          </button>
          <h1>📄 Загрузка документов</h1>
        </div>

        <div className="upload-documents-content">
          <div className="upload-documents-form">
            <p className="form-description">
              Для получения доступа к водительскому режиму необходимо загрузить следующие документы:
            </p>

            {renderFileInput(
              'Водительское удостоверение (лицевая сторона)',
              driverLicenseFront,
              setDriverLicenseFront,
              driverLicenseFrontRef
            )}

            {renderFileInput(
              'Водительское удостоверение (обратная сторона)',
              driverLicenseBack,
              setDriverLicenseBack,
              driverLicenseBackRef
            )}

            {renderFileInput(
              'СТС (лицевая сторона)',
              vehicleRegistrationFront,
              setVehicleRegistrationFront,
              vehicleRegistrationFrontRef
            )}

            {renderFileInput(
              'СТС (обратная сторона)',
              vehicleRegistrationBack,
              setVehicleRegistrationBack,
              vehicleRegistrationBackRef
            )}

            {renderFileInput(
              'Аватарка',
              avatar,
              setAvatar,
              avatarRef
            )}

            {error && (
              <div className="error-message">
                {error}
              </div>
            )}

            <button
              className="btn submit-documents-btn"
              onClick={handleSubmit}
              disabled={submitting}
            >
              {submitting ? (
                <>
                  🔄 Отправка...
                </>
              ) : (
                '📤 Отправить на проверку'
              )}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

