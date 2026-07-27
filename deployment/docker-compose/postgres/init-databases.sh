#!/bin/bash
set -euo pipefail

# HBP — инициализация Postgres: создаёт роль и БД для каждого микросервиса.
# Запускается один раз при первом старте контейнера (монтируется в
# /docker-entrypoint-initdb.d/). Скрипт идемпотентен.
#
# Для каждого сервиса создаются:
#   - роль <service>_service LOGIN с dev-паролем
#   - БД <service>_db с владельцем <service>_service
#   - grants на схему public + default privileges для будущих таблиц/sequence
#
# Пароли читаются из переменных окружения контейнера. Значения по умолчанию —
# только для dev; в проде их использовать нельзя.

SERVICES=(
    "hotel_db|hotel_service|${HOTEL_SERVICE_PASSWORD:-}"
    "inventory_db|inventory_service|${INVENTORY_SERVICE_PASSWORD:-}"
    "booking_db|booking_service|${BOOKING_SERVICE_PASSWORD:-}"
    "pricing_db|pricing_service|${PRICING_SERVICE_PASSWORD:-}"
    "payment_db|payment_service|${PAYMENT_SERVICE_PASSWORD:-}"
    "guest_db|guest_service|${GUEST_SERVICE_PASSWORD:-}"
    "notification_db|notification_service|${NOTIFICATION_SERVICE_PASSWORD:-}"
    "reporting_db|reporting_service|${REPORTING_SERVICE_PASSWORD:-}"
)

create_role() {
    local role="$1"
    local password="$2"
    psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname=postgres <<EOSQL
DO \$\$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = '${role}') THEN
        EXECUTE format('CREATE ROLE %I LOGIN PASSWORD %L', '${role}', '${password}');
    END IF;
END
\$\$;
EOSQL
}

create_database() {
    local db="$1"
    local owner="$2"
    psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname=postgres <<EOSQL
SELECT format('CREATE DATABASE %I OWNER %I', '${db}', '${owner}')
 WHERE NOT EXISTS (SELECT 1 FROM pg_database WHERE datname = '${db}')
\gexec
EOSQL
}

grant_privileges() {
    local db="$1"
    local role="$2"
    psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname="${db}" <<EOSQL
GRANT ALL ON SCHEMA public TO ${role};
ALTER DEFAULT PRIVILEGES FOR ROLE ${role} IN SCHEMA public
    GRANT ALL ON TABLES TO ${role};
ALTER DEFAULT PRIVILEGES FOR ROLE ${role} IN SCHEMA public
    GRANT ALL ON SEQUENCES TO ${role};
EOSQL
}

for entry in "${SERVICES[@]}"; do
    IFS='|' read -r db role password <<< "$entry"

    if [ -z "${password}" ]; then
        echo "ОШИБКА: пароль для роли '${role}' пустой. Проверьте *_SERVICE_PASSWORD в окружении." >&2
        exit 1
    fi

    echo ">>> ${db} (роль: ${role})"
    create_role "${role}" "${password}"
    create_database "${db}" "${role}"
    grant_privileges "${db}" "${role}"
done

echo ">>> Все базы и роли инициализированы."
