# ADR-006: Transactional Outbox

## Контекст

Необходимо гарантировать доставку событий в Kafka.

## Варианты

- Transactional Outbox
- CDC (Debezium)
- Two-Phase Commit

## Решение

Transactional Outbox.

## Обоснование

1. Гарантированная доставка (at-least-once)
2. Простота реализации
3. Надежность при падении сервиса
4. Возможность Replay

## Последствия

- Таблица outbox_messages
- Фоновый Polling Publisher
- Задержка доставки