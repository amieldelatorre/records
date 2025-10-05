#!/usr/bin/env bash
set -eou pipefail
set -x

REMOTE=${1:?REMOTE positional argument is required \'install.sh $REMOTE\'}
REMOTE_DOCKER_DATA_DIR=${2:-/docker-data}
REMOTE_USER=$(ssh "${REMOTE}" whoami)
REMOTE_DOCKER_ENV_FILE="${REMOTE_DOCKER_DATA_DIR}/.env"

ssh "${REMOTE}" "sudo mkdir -p ${REMOTE_DOCKER_DATA_DIR}/{data-grafana,data-prometheus,data-loki}"
ssh "${REMOTE}" sudo chown -R 472 "${REMOTE_DOCKER_DATA_DIR}/data-grafana"
ssh "${REMOTE}" sudo chown -R 1000:1000 "${REMOTE_DOCKER_DATA_DIR}/data-prometheus"
ssh "${REMOTE}" sudo chown -R 10001:10001 "${REMOTE_DOCKER_DATA_DIR}/data-loki"
ssh "${REMOTE}" sudo chown "${REMOTE_USER}" "${REMOTE_DOCKER_DATA_DIR}"
scp -r ./container_configs "${REMOTE}:${REMOTE_DOCKER_DATA_DIR}"
scp compose.yaml "${REMOTE}:${REMOTE_DOCKER_DATA_DIR}"
ssh "${REMOTE}" "[ -e '${REMOTE_DOCKER_ENV_FILE}' ]" || scp .env.sample "${REMOTE}:${REMOTE_DOCKER_ENV_FILE}"
# ssh "${REMOTE}" "cd ${REMOTE_DOCKER_DATA_DIR} && docker compose pull"

set +x
echo "Post install steps"
echo "1. ssh ${REMOTE}" 
echo "2. cd ${REMOTE_DOCKER_DATA_DIR}"
echo "3. Uncomment the tempo-init service in the compose.yaml file"
echo "4. Uncomment the dependency of the tempo serice on tempo-init in compose.yaml"
echo "5. Edit the ${REMOTE_DOCKER_ENV_FILE} with your own values"
echo "6. docker compose up -d"
echo "7. Comment out the tempo-init service"
echo "8. Comment out the dependency of the tempo serice on tempo-init in compose.yaml"
echo "9. docker compose up --remove-orphans -d"