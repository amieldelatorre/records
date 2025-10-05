#!/usr/bin/env bash
set -eou pipefail
set -x

SCRIPT_ROOT=$(dirname "${BASH_SOURCE[0]}")
PROJECT_ROOT="${SCRIPT_ROOT}/.."
# Go to project root
cd "${PROJECT_ROOT}"

docker compose down 
sleep_time=20
rm -rf ./.docker_data
sleep 2

docker compose up app-db -d
echo "Sleeping for ${sleep_time}s to let containers start up successfully" 
sleep "${sleep_time}"

filepath="${PROJECT_ROOT}/src/backend/IntegrationTests/Data/app_postgres_dump.sql"
cat "${filepath}"  | docker exec -i app-db psql -U records

docker compose up -d
