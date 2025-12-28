import React, { useState, useEffect, useCallback } from 'react';
import TelegramWebAppInfo from './TelegramWebAppInfo';
import axios from 'axios';
import { config, getInitData } from '../config';

interface ModerationDetailProps {
  isTelegramWebApp: boolean;
  documentsId: number;
  onBack: () => void;
  onApproved: () => void;
  onRejected: () => void;
}

interface DocumentDetails {
  id: number;
  userId: number;
  userName: string;
  status: string;
  statusName: string;
  submittedAt: string;
  verifiedAt: string | null;
  adminComment: string | null;
  driverLastName: string | null;
  driverFirstName: string | null;
  driverMiddleName: string | null;
  vehicleBrand: string | null;
  vehicleModel: string | null;
  vehicleColor: string | null;
  vehicleLicensePlate: string | null;
  driverLicenseFrontUrl: string | null;
  driverLicenseBackUrl: string | null;
  vehicleRegistrationFrontUrl: string | null;
  vehicleRegistrationBackUrl: string | null;
  avatarUrl: string | null;
}

export const ModerationDetail: React.FC<ModerationDetailProps> = ({
  isTelegramWebApp,
  documentsId,
  onBack,
  onApproved,
  onRejected,
}) => {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [document, setDocument] = useState<DocumentDetails | null>(null);
  const [submitting, setSubmitting] = useState(false);

  // Форма
  const [driverLastName, setDriverLastName] = useState('');
  const [driverFirstName, setDriverFirstName] = useState('');
  const [driverMiddleName, setDriverMiddleName] = useState('');
  const [vehicleBrand, setVehicleBrand] = useState('');
  const [vehicleModel, setVehicleModel] = useState('');
  const [vehicleColor, setVehicleColor] = useState('');
  const [vehicleLicensePlate, setVehicleLicensePlate] = useState('');
  const [adminComment, setAdminComment] = useState('');
  const [selectedImage, setSelectedImage] = useState<string | null>(null);

  // Логируем изменения selectedImage для отладки
  React.useEffect(() => {
    if (selectedImage) {
      console.log('Selected image changed to:', selectedImage);
    }
  }, [selectedImage]);

  const loadDocumentDetails = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const initData = getInitData();
      if (!initData) {
        throw new Error('Не удалось получить данные авторизации');
      }

      const response = await axios.post(
        `${config.apiBaseUrl}/api/webapp/moderation/document-details?initData=${encodeURIComponent(initData)}&documentsId=${documentsId}`
      );
      const doc = response.data;
      setDocument(doc);
      
      // Логируем URL изображений для отладки
      console.log('Document details loaded:', {
        driverLicenseFrontUrl: doc.driverLicenseFrontUrl,
        driverLicenseBackUrl: doc.driverLicenseBackUrl,
        vehicleRegistrationFrontUrl: doc.vehicleRegistrationFrontUrl,
        vehicleRegistrationBackUrl: doc.vehicleRegistrationBackUrl,
        avatarUrl: doc.avatarUrl
      });

      // Заполняем форму если данные уже есть
      if (doc.driverLastName) setDriverLastName(doc.driverLastName);
      if (doc.driverFirstName) setDriverFirstName(doc.driverFirstName);
      if (doc.driverMiddleName) setDriverMiddleName(doc.driverMiddleName);
      if (doc.vehicleBrand) setVehicleBrand(doc.vehicleBrand);
      if (doc.vehicleModel) setVehicleModel(doc.vehicleModel);
      if (doc.vehicleColor) setVehicleColor(doc.vehicleColor);
      if (doc.vehicleLicensePlate) setVehicleLicensePlate(doc.vehicleLicensePlate);
      if (doc.adminComment) setAdminComment(doc.adminComment);
    } catch (err: any) {
      console.error('Error loading document details:', err);
      const errorMessage = err.response?.data?.error || err.message || 'Ошибка при загрузке документа';
      setError(errorMessage);
      alert(errorMessage);
    } finally {
      setLoading(false);
    }
  }, [documentsId]);

  useEffect(() => {
    loadDocumentDetails();
  }, [loadDocumentDetails]);

  const handleApprove = async () => {
    // Валидация
    if (!driverLastName.trim() || !driverFirstName.trim() || !driverMiddleName.trim() ||
        !vehicleBrand.trim() || !vehicleModel.trim() || !vehicleColor.trim() || !vehicleLicensePlate.trim()) {
      const errorMsg = 'Все поля должны быть заполнены';
      setError(errorMsg);
      alert(errorMsg);
      return;
    }

    setSubmitting(true);
    setError(null);
    try {
      const initData = getInitData();
      if (!initData) {
        throw new Error('Не удалось получить данные авторизации');
      }

      const response = await axios.post(
        `${config.apiBaseUrl}/api/webapp/moderation/approve?initData=${encodeURIComponent(initData)}`,
        {
          documentsId: documentsId,
          driverLastName: driverLastName.trim(),
          driverFirstName: driverFirstName.trim(),
          driverMiddleName: driverMiddleName.trim(),
          vehicleBrand: vehicleBrand.trim(),
          vehicleModel: vehicleModel.trim(),
          vehicleColor: vehicleColor.trim(),
          vehicleLicensePlate: vehicleLicensePlate.trim(),
        }
      );

      if (response.data) {
        alert('Документы успешно одобрены');
        onApproved();
      }
    } catch (err: any) {
      console.error('Error approving documents:', err);
      const errorMessage = err.response?.data?.error || err.message || 'Ошибка при одобрении документов';
      setError(errorMessage);
      alert(errorMessage);
    } finally {
      setSubmitting(false);
    }
  };

  const handleReject = async () => {
    // Валидация комментария
    if (!adminComment.trim()) {
      const errorMsg = 'Комментарий администратора обязателен при отклонении';
      setError(errorMsg);
      alert(errorMsg);
      return;
    }

    setSubmitting(true);
    setError(null);
    try {
      const initData = getInitData();
      if (!initData) {
        throw new Error('Не удалось получить данные авторизации');
      }

      const response = await axios.post(
        `${config.apiBaseUrl}/api/webapp/moderation/reject?initData=${encodeURIComponent(initData)}`,
        {
          documentsId: documentsId,
          adminComment: adminComment.trim(),
        }
      );

      if (response.data) {
        alert('Документы отклонены');
        onRejected();
      }
    } catch (err: any) {
      console.error('Error rejecting documents:', err);
      const errorMessage = err.response?.data?.error || err.message || 'Ошибка при отклонении документов';
      setError(errorMessage);
      alert(errorMessage);
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) {
    return (
      <div className="app">
        <TelegramWebAppInfo isTelegramWebApp={isTelegramWebApp} />
        <div className="page-container">
          <div className="page-header">
            <button onClick={onBack} className="back-btn">
              ← Назад
            </button>
            <h1>Проверка документов</h1>
          </div>
          <div className="loading">
            <div className="spinner"></div>
            <p>Загрузка документа...</p>
          </div>
        </div>
      </div>
    );
  }

  if (error && !document) {
    return (
      <div className="app">
        <TelegramWebAppInfo isTelegramWebApp={isTelegramWebApp} />
        <div className="page-container">
          <div className="page-header">
            <button onClick={onBack} className="back-btn">
              ← Назад
            </button>
            <h1>Проверка документов</h1>
          </div>
          <div className="error-state">
            <div className="error-icon"></div>
            <h3>Ошибка</h3>
            <p>{error}</p>
            <button onClick={loadDocumentDetails} className="btn">
              Попробовать снова
            </button>
          </div>
        </div>
      </div>
    );
  }

  if (!document) {
    return null;
  }

  return (
    <div className="app">
      <TelegramWebAppInfo isTelegramWebApp={isTelegramWebApp} />
      <div className="page-container">
        <div className="page-header">
          <button onClick={onBack} className="back-btn">
            ← Назад
          </button>
          <h1>Проверка документов</h1>
        </div>

        <div className="moderation-detail-content">
          <div className="moderation-detail-form">
            <div className="document-user-info">
              <h3>Пользователь: {document.userName}</h3>
              <p className="document-status-text">
                Статус: <strong>{document.statusName}</strong>
              </p>
            </div>

            <div className="documents-preview">
              <h3>📸 Документы</h3>
              <div className="documents-grid">
                {document.driverLicenseFrontUrl && (
                  <div className="document-preview-item">
                    <label>Водительское удостоверение (лицевая сторона)</label>
                    <img 
                      src={document.driverLicenseFrontUrl} 
                      alt="ВУ лицевая" 
                      className="document-image"
                      onClick={(e) => {
                        e.preventDefault();
                        e.stopPropagation();
                        console.log('Image clicked:', document.driverLicenseFrontUrl);
                        if (document.driverLicenseFrontUrl) {
                          setSelectedImage(document.driverLicenseFrontUrl);
                        }
                      }}
                      onError={(e) => {
                        console.error('Error loading image:', document.driverLicenseFrontUrl);
                        const target = e.target as HTMLImageElement;
                        target.style.display = 'none';
                      }}
                      onLoad={() => {
                        console.log('Image loaded successfully:', document.driverLicenseFrontUrl);
                      }}
                      style={{ cursor: 'pointer' }}
                    />
                  </div>
                )}
                {document.driverLicenseBackUrl && (
                  <div className="document-preview-item">
                    <label>Водительское удостоверение (обратная сторона)</label>
                    <img 
                      src={document.driverLicenseBackUrl} 
                      alt="ВУ обратная" 
                      className="document-image"
                      onClick={(e) => {
                        e.preventDefault();
                        e.stopPropagation();
                        console.log('Image clicked:', document.driverLicenseBackUrl);
                        if (document.driverLicenseBackUrl) {
                          setSelectedImage(document.driverLicenseBackUrl);
                        }
                      }}
                      onError={(e) => {
                        console.error('Error loading image:', document.driverLicenseBackUrl);
                        const target = e.target as HTMLImageElement;
                        target.style.display = 'none';
                      }}
                      style={{ cursor: 'pointer' }}
                    />
                  </div>
                )}
                {document.vehicleRegistrationFrontUrl && (
                  <div className="document-preview-item">
                    <label>СТС (лицевая сторона)</label>
                    <img 
                      src={document.vehicleRegistrationFrontUrl} 
                      alt="СТС лицевая" 
                      className="document-image"
                      onClick={(e) => {
                        e.preventDefault();
                        e.stopPropagation();
                        console.log('Image clicked:', document.vehicleRegistrationFrontUrl);
                        if (document.vehicleRegistrationFrontUrl) {
                          setSelectedImage(document.vehicleRegistrationFrontUrl);
                        }
                      }}
                      onError={(e) => {
                        console.error('Error loading image:', document.vehicleRegistrationFrontUrl);
                        const target = e.target as HTMLImageElement;
                        target.style.display = 'none';
                      }}
                      style={{ cursor: 'pointer' }}
                    />
                  </div>
                )}
                {document.vehicleRegistrationBackUrl && (
                  <div className="document-preview-item">
                    <label>СТС (обратная сторона)</label>
                    <img 
                      src={document.vehicleRegistrationBackUrl} 
                      alt="СТС обратная" 
                      className="document-image"
                      onClick={(e) => {
                        e.preventDefault();
                        e.stopPropagation();
                        console.log('Image clicked:', document.vehicleRegistrationBackUrl);
                        if (document.vehicleRegistrationBackUrl) {
                          setSelectedImage(document.vehicleRegistrationBackUrl);
                        }
                      }}
                      onError={(e) => {
                        console.error('Error loading image:', document.vehicleRegistrationBackUrl);
                        const target = e.target as HTMLImageElement;
                        target.style.display = 'none';
                      }}
                      style={{ cursor: 'pointer' }}
                    />
                  </div>
                )}
                {document.avatarUrl && (
                  <div className="document-preview-item">
                    <label>Аватарка</label>
                    <img 
                      src={document.avatarUrl} 
                      alt="Аватарка" 
                      className="document-image"
                      onClick={(e) => {
                        e.preventDefault();
                        e.stopPropagation();
                        console.log('Image clicked:', document.avatarUrl);
                        if (document.avatarUrl) {
                          setSelectedImage(document.avatarUrl);
                        }
                      }}
                      onError={(e) => {
                        console.error('Error loading image:', document.avatarUrl);
                        const target = e.target as HTMLImageElement;
                        target.style.display = 'none';
                      }}
                      style={{ cursor: 'pointer' }}
                    />
                  </div>
                )}
              </div>
            </div>

            <div className="moderation-form-fields">
              <h3>📝 Информация для заполнения</h3>
              
              <div className="form-group">
                <label>Фамилия водителя *</label>
                <input
                  type="text"
                  value={driverLastName}
                  onChange={(e) => setDriverLastName(e.target.value)}
                  placeholder="Иванов"
                />
              </div>

              <div className="form-group">
                <label>Имя водителя *</label>
                <input
                  type="text"
                  value={driverFirstName}
                  onChange={(e) => setDriverFirstName(e.target.value)}
                  placeholder="Иван"
                />
              </div>

              <div className="form-group">
                <label>Отчество водителя *</label>
                <input
                  type="text"
                  value={driverMiddleName}
                  onChange={(e) => setDriverMiddleName(e.target.value)}
                  placeholder="Иванович"
                />
              </div>

              <div className="form-group">
                <label>Марка автомобиля *</label>
                <input
                  type="text"
                  value={vehicleBrand}
                  onChange={(e) => setVehicleBrand(e.target.value)}
                  placeholder="Toyota"
                />
              </div>

              <div className="form-group">
                <label>Модель автомобиля *</label>
                <input
                  type="text"
                  value={vehicleModel}
                  onChange={(e) => setVehicleModel(e.target.value)}
                  placeholder="Camry"
                />
              </div>

              <div className="form-group">
                <label>Цвет автомобиля *</label>
                <input
                  type="text"
                  value={vehicleColor}
                  onChange={(e) => setVehicleColor(e.target.value)}
                  placeholder="Белый"
                />
              </div>

              <div className="form-group">
                <label>Государственный номер автомобиля *</label>
                <input
                  type="text"
                  value={vehicleLicensePlate}
                  onChange={(e) => setVehicleLicensePlate(e.target.value)}
                  placeholder="А123БВ 02"
                />
              </div>

              <div className="form-group">
                <label>Комментарий администратора (при отклонении)</label>
                <textarea
                  value={adminComment}
                  onChange={(e) => setAdminComment(e.target.value)}
                  placeholder="Укажите причину отклонения..."
                  rows={4}
                />
              </div>
            </div>

            {error && (
              <div className="error-message">
                {error}
              </div>
            )}

            <div className="moderation-actions">
              <button
                className="btn btn-success approve-btn"
                onClick={handleApprove}
                disabled={submitting}
              >
                {submitting ? 'Одобрение...' : 'Одобрить'}
              </button>
              <button
                className="btn btn-danger reject-btn"
                onClick={handleReject}
                disabled={submitting}
              >
                {submitting ? 'Отклонение...' : 'Отклонить'}
              </button>
            </div>
          </div>
        </div>
      </div>

      {/* Модальное окно для полноэкранного просмотра изображения */}
      {selectedImage && (
        <div 
          className="image-modal-overlay"
          onClick={(e) => {
            e.preventDefault();
            e.stopPropagation();
            console.log('Modal overlay clicked, closing modal');
            setSelectedImage(null);
          }}
          style={{ 
            position: 'fixed',
            top: 0,
            left: 0,
            right: 0,
            bottom: 0,
            zIndex: 10000
          }}
        >
          <div 
            className="image-modal-content" 
            onClick={(e) => {
              e.preventDefault();
              e.stopPropagation();
            }}
            style={{ position: 'relative' }}
          >
            <button 
              className="image-modal-close"
              onClick={(e) => {
                e.preventDefault();
                e.stopPropagation();
                console.log('Close button clicked');
                setSelectedImage(null);
              }}
              style={{ 
                position: 'absolute',
                top: -40,
                right: 0,
                zIndex: 10001
              }}
            >
              ✕
            </button>
            <img 
              src={selectedImage} 
              alt="Полноэкранное изображение" 
              className="image-modal-image"
              onClick={(e) => {
                e.preventDefault();
                e.stopPropagation();
              }}
              onError={(e) => {
                console.error('Error loading modal image:', selectedImage);
                const target = e.target as HTMLImageElement;
                target.style.display = 'none';
              }}
              onLoad={() => {
                console.log('Modal image loaded successfully:', selectedImage);
              }}
            />
          </div>
        </div>
      )}
    </div>
  );
};

