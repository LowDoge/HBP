# 2. Функциональные требования

## 2.1. Hotel Service

**Ответственность:** CRUD операции для отелей и номеров.

Функции:
- Создание, редактирование, удаление отелей
- Управление номерами
- Управление статусами номеров (Active, Maintenance, Blocked)
- Bulk-импорт через CSV/JSON
- Кэширование каталога в Redis (TTL: 1 час)

Бизнес-правила:
- Отель не удаляется при наличии активных бронирований
- Номер не изменяется при наличии активных блокировок
- Номер уникален в пределах отеля

API:
- GET /api/v1/hotels
- GET /api/v1/hotels/{id}
- POST /api/v1/hotels
- PUT /api/v1/hotels/{id}
- DELETE /api/v1/hotels/{id}
- GET /api/v1/hotels/{id}/rooms
- POST /api/v1/hotels/{id}/rooms

---

## 2.2. Inventory Service

**Ответственность:** Управление доступностью номеров.

Функции:
- Проверка доступности на даты
- Блокировка/разблокировка номеров
- Автоматическое освобождение по TTL (15 минут)
- Управление овербукингом (max 10%)
- Кэширование доступности в Redis (TTL: 5 минут)

Бизнес-правила:
- Номер доступен, если не заблокирован и не зарезервирован
- Овербукинг не более 10%
- Блокировка автоматически освобождается через 15 минут

### gRPC Contract

```protobuf
service InventoryService {
  rpc CheckAvailability (AvailabilityRequest) returns (AvailabilityResponse);
  rpc LockRoom (LockRequest) returns (LockResponse);
  rpc ReleaseRoom (ReleaseRequest) returns (ReleaseResponse);
}
```

---

## 2.3. Booking Service

**Ответственность:** Создание и управление бронированиями.

Функции:
- Создание бронирования (Saga Choreography)
- Управление статусами (Draft -> PendingPayment -> Confirmed -> CheckedIn -> Completed)
- Отмена с расчетом штрафа
- Изменение дат и количества гостей
- Check-in/Check-out
- Идемпотентность (Idempotency Key через Redis)

Бизнес-правила:
- Минимальный срок: 1 ночь
- Максимальный срок: 90 дней
- Количество гостей не превышает вместимость номера
- Изменение дат только до Check-in
- Отмена: штраф 0% за N дней, 100% при поздней отмене

API:
- POST /api/v1/bookings
- GET /api/v1/bookings/{id}
- GET /api/v1/bookings
- PUT /api/v1/bookings/{id}
- DELETE /api/v1/bookings/{id}
- POST /api/v1/bookings/{id}/checkin
- POST /api/v1/bookings/{id}/checkout

---

## 2.4. Pricing Service

**Ответственность:** Динамическое ценообразование.

Факторы:
- Базовая цена
- Сезонный коэффициент (0.7 - 1.5)
- Коэффициент загрузки (0.8 - 2.0)
- Коэффициент дней до заезда (0.8 - 1.0)
- Скидка по уровню лояльности (0 - 20%)

Формула:
```
Цена = База * Сезон * Загрузка * (1 - Скидка)
```

API:
- GET /api/v1/pricing/calculate
- GET /api/v1/pricing/hotels/{id}/rooms/{id}

---

## 2.5. Payment Service

**Ответственность:** Обработка платежей.

Функции:
- Авторизация (Hold)
- Списание (Capture)
- Возврат (Refund)
- Токенизация данных карты (mock)

Статусы транзакций:
- Pending -> Authorized -> Captured -> Refunded
- Pending -> Declined
- Pending -> Failed

---

## 2.6. Guest Service

**Ответственность:** Управление профилями гостей.

Функции:
- CRUD операции
- Хранение персональных данных
- Управление предпочтениями
- История бронирований
- Программа лояльности

Уровни лояльности:
| Уровень | Баллы | Скидка |
| :--- | :--- | :--- |
| Bronze | 0-999 | 0% |
| Silver | 1000-4999 | 5% |
| Gold | 5000-19999 | 10% |
| Platinum | 20000+ | 20% |

---

## 2.7. Notification Service

**Ответственность:** Уведомления.

Каналы:
- Email
- SMS
- Webhook

Сценарии:
- Подтверждение бронирования
- Напоминание за 3 дня
- Изменение бронирования
- Отмена бронирования
- Check-in
- Check-out

---

## 2.8. Reporting Service

**Ответственность:** Отчеты и аналитика (CQRS).

Операционные отчеты:
- Occupancy Rate
- ADR (Average Daily Rate)
- RevPAR (Revenue Per Available Room)

Финансовые отчеты:
- Доходы по отелям
- Доходы по типам номеров
- Комиссии каналам продаж

Аналитика гостей:
- Демографический анализ
- Частота бронирований
- Предпочтения