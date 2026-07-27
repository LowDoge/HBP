# 5. Инфраструктура и деплой

## 5.1. Разработка (Docker Compose)

### Требования

- Docker 26+
- Docker Compose 2.27+

### Запуск

```bash
docker-compose -f deployment/docker-compose/docker-compose.yml up -d
```

### Компоненты

- PostgreSQL
- Redis
- Kafka
- Seq
- Prometheus + Grafana
- Jaeger

### Доступ

| Сервис            | URL / порт                  | Назначение                          |
| :---------------- | :-------------------------- | :---------------------------------- |
| API Gateway       | http://localhost:5000       | Точка входа для клиентов            |
| Swagger           | http://localhost:5000/swagger | OpenAPI UI                       |
| PostgreSQL        | localhost:5432              | БД микросервисов (8 баз)            |
| Redis             | localhost:6379              | Кэш + распределённые блокировки     |
| Kafka             | localhost:9092              | Event bus                           |
| Kafka UI          | http://localhost:8081       | UI для просмотра топиков (dev-only) |
| Seq               | http://localhost:5341       | Структурированные логи               |
| Prometheus        | http://localhost:9090       | Метрики                              |
| Grafana           | http://localhost:3000       | Дашборды (admin / admin)            |
| Jaeger UI         | http://localhost:16686      | Трассировка                          |
| Jaeger OTLP gRPC  | localhost:4317              | Приём трейсов (gRPC)                |
| Jaeger OTLP HTTP  | localhost:4318              | Приём трейсов (HTTP)                |

Порты микросервисов (при запуске через `dotnet run` на хосте) совпадают с scrape-таргетами Prometheus и лежат в диапазоне 5001–5008:

| Сервис              | Порт |
| :------------------ | :--- |
| Hotel Service       | 5001 |
| Inventory Service   | 5002 |
| Booking Service     | 5003 |
| Pricing Service     | 5004 |
| Payment Service     | 5005 |
| Guest Service       | 5006 |
| Notification Service| 5007 |
| Reporting Service   | 5008 |