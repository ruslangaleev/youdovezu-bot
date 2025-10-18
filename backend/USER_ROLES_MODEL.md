# Модель ролей и статусов пользователя YouDovezu

## Бизнес-логика

### 1. Начальное состояние пользователя
```
Пользователь регистрируется → SystemRole: User, CanBePassenger: false, CanBeDriver: false
```

### 2. После согласия с ПД
```
Пользователь соглашается с ПД → CanBePassenger: true
Теперь может быть пассажиром!
```

### 3. После подтверждения водительских данных
```
Пользователь подтверждает водительские данные → CanBeDriver: true, TrialStartDate/TrialEndDate
Теперь может быть водителем! (14 дней триала)
```

### 4. Системные роли (права доступа)
```
User (0)     - Обычный пользователь
Moderator (1) - Модератор (может модерировать контент)
Admin (2)    - Администратор (полные права)
```

## Возможности пользователя

### Пассажир
- Условие: `PrivacyConsent = true` AND `CanBePassenger = true` AND `Status = Active`
- Может: искать поездки, бронировать места, оставлять отзывы

### Водитель  
- Условие: `PrivacyConsent = true` AND `CanBeDriver = true` AND `Status = Active`
- Может: создавать поездки, принимать заявки, управлять поездками
- Триал: 14 дней после подтверждения водительских данных

### Модератор/Админ
- Условие: `SystemRole = Moderator OR Admin`
- Может: модерировать контент, управлять пользователями, просматривать статистику

## Примеры состояний

### Новый пользователь
```
SystemRole: User
CanBePassenger: false
CanBeDriver: false
Status: PendingRegistration
```

### Активный пассажир
```
SystemRole: User
CanBePassenger: true
CanBeDriver: false
Status: Active
```

### Активный водитель (триал)
```
SystemRole: User
CanBePassenger: true
CanBeDriver: true
Status: Active
TrialStartDate: 2024-01-01
TrialEndDate: 2024-01-15
```

### Модератор-водитель
```
SystemRole: Moderator
CanBePassenger: true
CanBeDriver: true
Status: Active
```

## Методы проверки

```csharp
user.CanActAsPassenger()  // Может ли быть пассажиром
user.CanActAsDriver()     // Может ли быть водителем
user.IsModerator()        // Является ли модератором
user.IsAdmin()           // Является ли администратором
user.IsTrialActive()     // Активен ли триал
```

