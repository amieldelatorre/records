#!/usr/bin/env bash
set -eou pipefail
set -x

SCRIPT_ROOT=$(dirname "${BASH_SOURCE[0]}")
PROJECT_ROOT="${SCRIPT_ROOT}/.."
# Go to project root
cd "${PROJECT_ROOT}"

docker compose down
docker compose up app-db -d

sleep_time=5
echo "Sleeping for ${sleep_time}s to let container start up successfully" 
sleep "${sleep_time}"

filepath="${PROJECT_ROOT}/src/backend/IntegrationTests/Data/app_postgres_dump.sql"

# pgdump_all -c # Includes SQL commands to clean (drop) databases before recreating them
docker exec -t app-db pg_dump -U records > "${filepath}"

echo "App database dump has been created at ${filepath}"
