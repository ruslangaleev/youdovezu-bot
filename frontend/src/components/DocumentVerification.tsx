import React, { useEffect, useState } from 'react';
import TelegramWebAppInfo from './TelegramWebAppInfo';
import axios from 'axios';
import { config } from '../config';
import { getInitData } from '../config';

interface DocumentVerificationProps {
  isTelegramWebApp: boolean;
  onBack: () => void;
}

interface VerificationStatus {
  status: string;
  statusName: string;
  submittedAt: string;
  verifiedAt?: string;
  adminComment?: string;
}

export const DocumentVerification: React.FC<DocumentVerificationProps> = ({
  isTelegramWebApp,
  onBack,
}) => {
  const [status, setStatus] = useState<VerificationStatus | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadStatus();
  }, []);

  const loadStatus = async () => {
    setLoading(true);
    setError(null);

    try {
      const initData = getInitData();
      if (!initData) {
        throw new Error('Не удалось получить данные авторизации');
      }

      const response = await axios.post(
        `${config.apiBaseUrl}/api/webapp/driver-documents/status?initData=${encodeURIComponent(initData)}`
      );

      if (response.data.status === 'not_submitted') {
        setStatus(null);
      } else {
        setStatus(response.data);
      }
    } catch (err: any) {
      const errorMessage = err.response?.data?.error || err.message || 'Ошибка при загрузке статуса';
      setError(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  const getStatusIcon = (status: string) => {
    switch (status) {
      case 'Pending':
      case 'UnderReview':
        return '⏳';
      case 'Approved':
        return '✅';
      case 'Rejected':
        return '❌';
      default:
        return '📄';
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Pending':
      case 'UnderReview':
        return '#ffc107';
      case 'Approved':
        return '#28a745';
      case 'Rejected':
        return '#dc3545';
      default:
        return '#6c757d';
    }
  };

  return (
    <div className="app">
      <TelegramWebAppInfo isTelegramWebApp={isTelegramWebApp} />
      <div className="page-container">
        <div className="page-header">
          <button onClick={onBack} className="back-btn">
            ← Назад
          </button>
          <h1>📋 Проверка документов</h1>
        </div>

        <div className="verification-content">
          {loading ? (
            <div className="loading">
              <div className="spinner"></div>
              <p>Загрузка статуса проверки...</p>
            </div>
          ) : error ? (
            <div className="error-state">
              <div className="error-icon">⚠️</div>
              <h3>Ошибка</h3>
              <p>{error}</p>
              <button className="btn" onClick={loadStatus}>
                Попробовать снова
              </button>
            </div>
          ) : status ? (
            <div className="verification-status">
              <div className="status-icon" style={{ color: getStatusColor(status.status) }}>
                {getStatusIcon(status.status)}
              </div>
              <h2 style={{ color: getStatusColor(status.status) }}>
                {status.statusName}
              </h2>
              
              <div className="status-details">
                <div className="status-item">
                  <span className="status-label">Отправлено:</span>
                  <span className="status-value">
                    {new Date(status.submittedAt).toLocaleString('ru-RU', {
                      year: 'numeric',
                      month: 'long',
                      day: 'numeric',
                      hour: '2-digit',
                      minute: '2-digit'
                    })}
                  </span>
                </div>

                {status.verifiedAt && (
                  <div className="status-item">
                    <span className="status-label">Проверено:</span>
                    <span className="status-value">
                      {new Date(status.verifiedAt).toLocaleString('ru-RU', {
                        year: 'numeric',
                        month: 'long',
                        day: 'numeric',
                        hour: '2-digit',
                        minute: '2-digit'
                      })}
                    </span>
                  </div>
                )}

                {status.adminComment && (
                  <div className="status-item admin-comment">
                    <span className="status-label">Комментарий администратора:</span>
                    <span className="status-value">{status.adminComment}</span>
                  </div>
                )}
              </div>

              {(status.status === 'Pending' || status.status === 'UnderReview') && (
                <div className="status-message">
                  <p>Ваши документы находятся на проверке. Мы свяжемся с вами после завершения проверки.</p>
                </div>
              )}

              {status.status === 'Approved' && (
                <div className="status-message success">
                  <p>🎉 Поздравляем! Ваши документы одобрены. Теперь вы можете использовать все возможности водительского режима.</p>
                </div>
              )}

              {status.status === 'Rejected' && (
                <div className="status-message error">
                  <p>К сожалению, ваши документы были отклонены. Пожалуйста, проверьте комментарий администратора и отправьте документы заново.</p>
                </div>
              )}
            </div>
          ) : (
            <div className="no-documents">
              <div className="no-documents-icon">📄</div>
              <h3>Документы не отправлены</h3>
              <p>Вы еще не отправили документы на проверку.</p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

