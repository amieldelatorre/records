#!/usr/bin/env bash
set -eou pipefail

SCRIPT_ROOT=$(dirname "${BASH_SOURCE[0]}")
PROJECT_ROOT="${SCRIPT_ROOT}/.."
# Go to project root
cd "${PROJECT_ROOT}"
sleep_time=5

docker compose down
echo "Sleeping for ${sleep_time}s to let container shutdown successfully" 
sleep "${sleep_time}"


rm -rf ./docker_data

docker compose up records-postgres-db unleash-postgres-db -d
echo "Sleeping for ${sleep_time}s to let container start up successfully" 
sleep "${sleep_time}"

recordsDumpFilepath="${PROJECT_ROOT}/src/backend/Records/IntegrationTests/Data/records_postgres_dump.sql"
unleashDumpFilepath="${PROJECT_ROOT}/src/backend/Records/IntegrationTests/Data/unleash_postgres_dump.sql"

cat "${recordsDumpFilepath}" | docker exec -i records-postgres-db psql -U root -d records
cat "${unleashDumpFilepath}" | docker exec -i unleash-postgres-db psql -U root -d unleash
