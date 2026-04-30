#!/bin/bash
set -e

psql -v ON_ERROR_STOP=1 --username "postgres" <<-EOSQL
    CREATE EXTENSION IF NOT EXISTS timescaledb CASCADE;
    ALTER USER "$POSTGRESQL_USER" WITH SUPERUSER;
EOSQL

DATABASES=("drivolution" "deviceData")

for DB in "${DATABASES[@]}"; do
    echo "Creating regular PostgreSQL database: $DB"
    PGPASSWORD=$POSTGRESQL_POSTGRES_PASSWORD psql -v ON_ERROR_STOP=1 --username "postgres" <<-EOSQL
        CREATE DATABASE "$DB"
        WITH TEMPLATE template0
        ENCODING 'UTF8';
        GRANT ALL PRIVILEGES ON DATABASE "$DB" TO "$POSTGRESQL_USER";
        
        \c "$DB";
        
        GRANT ALL PRIVILEGES ON SCHEMA public TO "$POSTGRESQL_USER";
        
        GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO "$POSTGRESQL_USER";
        
        GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO "$POSTGRESQL_USER";
EOSQL
done