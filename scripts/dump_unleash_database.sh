#!/usr/bin/env bash
set -eou pipefail

SCRIPT_ROOT=$(dirname "${BASH_SOURCE[0]}")
PROJECT_ROOT="${SCRIPT_ROOT}/.."
# Go to project root
cd "${PROJECT_ROOT}"

docker compose down
docker compose up unleash-postgres-db -d

sleep_time=5
echo "Sleeping for ${sleep_time}s to let container start up successfully" 
sleep "${sleep_time}"

filepath="${PROJECT_ROOT}/src/backend/Records/IntegrationTests/Data/unleash_postgres_dump.sql"

# pgdump_all -c # Includes SQL commands to clean (drop) databases before recreating them
docker exec -t unleash-postgres-db pg_dump -U root -d unleash --inserts >> "${filepath}"

echo "Unleash database dump has been created at ${filepath}"
