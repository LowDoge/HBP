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

| Сервис | URL |
| :--- | :--- |
| API Gateway | http://localhost:5000 |
| Swagger | http://localhost:5000/swagger |
| Seq | http://localhost:5341 |
| Prometheus | http://localhost:9090 |
| Grafana | http://localhost:3000 |
| Jaeger | http://localhost:16686 |