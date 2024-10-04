#!/usr/bin/env bash
set -eou pipefail

SCRIPT_ROOT=$(dirname "${BASH_SOURCE[0]}")
PROJECT_ROOT="${SCRIPT_ROOT}/.."
# Go to project root
cd "${PROJECT_ROOT}"

docker compose down
docker compose up records-postgres-db -d

sleep_time=5
echo "Sleeping for ${sleep_time}s to let container start up successfully" 
sleep "${sleep_time}"

filepath="${PROJECT_ROOT}/src/backend/Records/IntegrationTests/Data/records_postgres_dump.sql"

# pgdump_all -c # Includes SQL commands to clean (drop) databases before recreating them
docker exec -t records-postgres-db pg_dump -U root -d records --inserts > "${filepath}"

echo "Records database dump has been created at ${filepath}"
