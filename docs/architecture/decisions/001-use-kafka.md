# ADR-001: Kafka

## Контекст

Необходим брокер сообщений для асинхронного взаимодействия микросервисов.

## Варианты

- RabbitMQ
- Apache Kafka
- Azure Service Bus

## Решение

Apache Kafka.

## Обоснование

1. Хранение событий с retention
2. Replay для восстановления состояния
3. Высокая пропускная способность
4. Партиционирование по hotel_id

## Последствия

- Сложная настройка
- Idempotent handlers (at-least-once)
- Мониторинг Consumer Lag