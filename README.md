# 🏨 Hotel Booking & Revenue Management Platform

## 📖 О проекте

**Hotel Booking Platform** — микросервисная платформа для управления бронированием номеров в отелях с функцией динамического ценообразования.

---

### Ключевые особенности

| Особенность | Описание |
| :--- | :--- |
| 🏗️ **Микросервисная архитектура** | 8 независимых сервисов с собственными БД |
| 📨 **Event-Driven** | Асинхронное взаимодействие через Apache Kafka 3.9 |
| 🔄 **Saga Pattern** | Распределенные транзакции с компенсирующими операциями |
| 📦 **Transactional Outbox** | Гарантированная доставка событий |
| 📊 **CQRS** | Разделение на команды и запросы в Reporting Service |
| 💰 **Dynamic Pricing** | Ценообразование на основе спроса и сезонности |
| 🔍 **Observability** | Логи (Seq), метрики (Prometheus 3.0), трассировка (Jaeger 1.60) |

### Технологический стек

| Компонент | Технология | Версия |
| :--- | :--- | :--- |
| **Framework** | ASP.NET Core | 10.0 |
| **API** | REST + gRPC | gRPC 2.65 |
| **ORM** | Dapper | 2.1.35 |
| **Database** | PostgreSQL | 17 |
| **Cache** | Redis | 7.4 |
| **Message Broker** | Apache Kafka | 3.9 (KRaft) |
| **Logging** | Serilog + Seq | Serilog 4.2 |
| **Metrics** | Prometheus + Grafana | 3.0 / 11.3 |
| **Tracing** | OpenTelemetry + Jaeger | 1.9 / 1.60 |
| **Testing** | xUnit + Moq + k6 | xUnit 2.9 |

---

## 🏗️ Архитектура

### Диаграмма контейнеров (C4)

```mermaid
graph TB
    subgraph External["External World"]
        Client[Web/Mobile Clients]
    end
    
    subgraph Gateway["API Gateway Layer"]
        YARP[YARP API Gateway<br/>Routing, Auth, Rate Limiting]
    end
    
    subgraph Services["Microservices"]
        Hotel[Hotel Service<br/>Управление отелями]
        Inventory[Inventory Service<br/>Управление доступностью]
        Booking[Booking Service<br/>Бронирования + Saga]
        Pricing[Pricing Service<br/>Динамическое ценообразование]
        Payment[Payment Service<br/>Платежи]
        Guest[Guest Service<br/>Профили гостей]
        Notification[Notification Service<br/>Уведомления]
        Reporting[Reporting Service<br/>Отчеты + CQRS]
    end
    
    subgraph Data["Data Layer"]
        PG1[(PostgreSQL 17<br/>Hotel DB)]
        PG2[(PostgreSQL 17<br/>Inventory DB)]
        PG3[(PostgreSQL 17<br/>Booking DB)]
        PG4[(PostgreSQL 17<br/>Pricing DB)]
        PG5[(PostgreSQL 17<br/>Payment DB)]
        PG6[(PostgreSQL 17<br/>Guest DB)]
        PG7[(PostgreSQL 17<br/>Notification DB)]
        PG8[(PostgreSQL 17<br/>Reporting DB)]
        Redis[(Redis 7.4<br/>Cache + Locks)]
    end
    
    subgraph Messaging["Messaging"]
        Kafka[Apache Kafka 3.9<br/>Event Bus]
    end
    
    Client --> YARP
    YARP --> Hotel & Inventory & Booking & Pricing & Payment & Guest
    
    Booking -->|gRPC| Inventory
    Booking -->|gRPC| Payment
    Booking -->|gRPC| Pricing
    Booking -->|gRPC| Guest
    
    Hotel --> PG1
    Inventory --> PG2
    Booking --> PG3
    Pricing --> PG4
    Payment --> PG5
    Guest --> PG6
    Notification --> PG7
    Reporting --> PG8
    
    Hotel -.->|Cache| Redis
    Inventory -.->|Cache + Locks| Redis
    Booking -.->|Cache| Redis
    
    Booking -->|Publish Events| Kafka
    Inventory -->|Publish Events| Kafka
    Payment -->|Publish Events| Kafka
    Guest -->|Publish Events| Kafka
    
    Kafka -->|Consume| Notification
    Kafka -->|Consume| Reporting
```
