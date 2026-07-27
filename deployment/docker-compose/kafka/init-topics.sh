#!/bin/bash
set -euo pipefail

# HBP — инициализация Kafka-топиков.
# Создаёт топики после старта брокера. Идемпотентен: существующие топики
# пропускаются. Список топиков соответствует docs/specifications/04-system-design.md.

# В образе apache/kafka PATH пустой — добавляем каталог с утилитами.
export PATH="/opt/kafka/bin:${PATH:-}"

BROKER="${KAFKA_BROKER:-kafka:9092}"
PARTITIONS="${KAFKA_PARTITIONS:-3}"
REPLICATION="${KAFKA_REPLICATION:-1}"
RETENTION_MS="${KAFKA_RETENTION_MS:-604800000}"   # 7 дней

TOPICS=(
    "booking.created"
    "booking.confirmed"
    "booking.cancelled"
    "booking.modified"
    "inventory.locked"
    "inventory.released"
    "payment.authorized"
    "payment.captured"
    "payment.refunded"
    "pricing.updated"
    "guest.registered"
)

echo "Ожидаем Kafka на ${BROKER}..."
for i in {1..30}; do
    if kafka-topics.sh --bootstrap-server "${BROKER}" --list >/dev/null 2>&1; then
        echo "Kafka доступна."
        break
    fi
    if [ "${i}" -eq 30 ]; then
        echo "ОШИБКА: Kafka не отвечает после 30 попыток." >&2
        exit 1
    fi
    sleep 2
done

for topic in "${TOPICS[@]}"; do
    if kafka-topics.sh --bootstrap-server "${BROKER}" --list 2>/dev/null \
        | grep -qx "${topic}"; then
        echo "Топик уже существует: ${topic}"
        continue
    fi

    echo "Создаём топик: ${topic} (partitions=${PARTITIONS}, rf=${REPLICATION})"
    kafka-topics.sh --bootstrap-server "${BROKER}" \
        --create --if-not-exists \
        --topic "${topic}" \
        --partitions "${PARTITIONS}" \
        --replication-factor "${REPLICATION}" \
        --config retention.ms="${RETENTION_MS}" \
        --config min.insync.replicas=1
done

echo ">>> Инициализация топиков завершена."
