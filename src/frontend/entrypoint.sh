#!/usr/bin/env bash
set -eoux pipefail

escaped_app_api_host=$(echo "${APP__API_HOST:?required environment variable}" | sed 's/[&\/]/\\&/g')
sed -i "s|__API_HOST__|${escaped_app_api_host}|g" /usr/share/nginx/html/scripts/env.js
exec /docker-entrypoint.sh "$@" 