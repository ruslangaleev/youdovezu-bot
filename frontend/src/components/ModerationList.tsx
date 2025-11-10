import React, { useState, useEffect } from 'react';
import TelegramWebAppInfo from './TelegramWebAppInfo';
import axios from 'axios';
import { config, getInitData } from '../config';

interface ModerationListProps {
  isTelegramWebApp: boolean;
  onBack: () => void;
  onSelectDocument: (documentsId: number) => void;
}

interface DocumentItem {
  id: number;
  userId: number;
  userName: string;
  status: string;
  statusName: string;
  submittedAt: string;
  createdAt: string;
}

export const ModerationList: React.FC<ModerationListProps> = ({
  isTelegramWebApp,
  onBack,
  onSelectDocument,
}) => {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [documents, setDocuments] = useState<DocumentItem[]>([]);

  useEffect(() => {
    loadDocuments();
  }, []);

  const loadDocuments = async () => {
    setLoading(true);
    setError(null);
    try {
      const initData = getInitData();
      if (!initData) {
        throw new Error('Не удалось получить данные авторизации');
      }

      const response = await axios.post(
        `${config.apiBaseUrl}/api/webapp/moderation/documents-list?initData=${encodeURIComponent(initData)}`
      );
      setDocuments(response.data);
    } catch (err: any) {
      console.error('Error loading documents:', err);
      const errorMessage = err.response?.data?.error || err.message || 'Ошибка при загрузке документов';
      setError(errorMessage);
      alert(errorMessage);
    } finally {
      setLoading(false);
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
            <h1>📋 Модерация документов</h1>
          </div>
          <div className="loading">
            <div className="spinner"></div>
            <p>Загрузка документов...</p>
          </div>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="app">
        <TelegramWebAppInfo isTelegramWebApp={isTelegramWebApp} />
        <div className="page-container">
          <div className="page-header">
            <button onClick={onBack} className="back-btn">
              ← Назад
            </button>
            <h1>📋 Модерация документов</h1>
          </div>
          <div className="error-state">
            <div className="error-icon">⚠️</div>
            <h3>Ошибка</h3>
            <p>{error}</p>
            <button onClick={loadDocuments} className="btn">
              Попробовать снова
            </button>
          </div>
        </div>
      </div>
    );
  }

  if (documents.length === 0) {
    return (
      <div className="app">
        <TelegramWebAppInfo isTelegramWebApp={isTelegramWebApp} />
        <div className="page-container">
          <div className="page-header">
            <button onClick={onBack} className="back-btn">
              ← Назад
            </button>
            <h1>📋 Модерация документов</h1>
          </div>
          <div className="empty-state">
            <div className="empty-icon">📄</div>
            <h3>Нет документов на проверке</h3>
            <p>Все документы проверены или нет новых заявок</p>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="app">
      <TelegramWebAppInfo isTelegramWebApp={isTelegramWebApp} />
      <div className="page-container">
        <div className="page-header">
          <button onClick={onBack} className="back-btn">
            ← Назад
          </button>
          <h1>📋 Модерация документов</h1>
        </div>

        <div className="moderation-list-content">
          <div className="documents-list">
            {documents.map((doc) => (
              <div
                key={doc.id}
                className="document-item"
                onClick={() => onSelectDocument(doc.id)}
              >
                <div className="document-header">
                  <div className="document-user">
                    <span className="user-icon">👤</span>
                    <span className="user-name">{doc.userName}</span>
                  </div>
                  <div className={`document-status status-${doc.status.toLowerCase()}`}>
                    {doc.statusName}
                  </div>
                </div>
                <div className="document-info">
                  <div className="document-date">
                    <span className="date-label">Отправлено:</span>
                    <span className="date-value">
                      {new Date(doc.submittedAt).toLocaleString('ru-RU', {
                        year: 'numeric',
                        month: 'long',
                        day: 'numeric',
                        hour: '2-digit',
                        minute: '2-digit'
                      })}
                    </span>
                  </div>
                </div>
                <div className="document-action">
                  <button className="btn btn-primary">
                    Проверить →
                  </button>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
};

