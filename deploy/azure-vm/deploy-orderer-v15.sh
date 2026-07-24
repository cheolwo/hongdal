#!/usr/bin/env bash
set -euo pipefail

release_directory="${1:?release directory is required}"
application_root="${2:-/opt/ssalddel}"
timestamp="$(date -u +%Y%m%dT%H%M%SZ)"
web_next="$application_root/web-next-orderer-v15-$timestamp"
web_backup="$application_root/web-before-orderer-v15-$timestamp"
compose_file="$application_root/compose.yaml"
override_file="$application_root/compose.orderer-v15.override.yaml"
environment_file="$application_root/.env"

case "$application_root" in
  /opt/ssalddel) ;;
  *)
    echo "Application root must be /opt/ssalddel." >&2
    exit 1
    ;;
esac

for required_file in \
  "$release_directory/ssalddel-server.tar" \
  "$release_directory/web.tar.gz" \
  "$release_directory/compose.orderer-v15.override.yaml" \
  "$compose_file" \
  "$environment_file"; do
  if [[ ! -f "$required_file" ]]; then
    echo "Required deployment file is missing: $required_file" >&2
    exit 1
  fi
done

old_image="$(docker inspect ssalddel-preview-app-1 --format '{{.Image}}')"
rollback_tag="ssalddel-server:azure-preview-rollback-$timestamp"
docker tag "$old_image" "$rollback_tag"
docker load --input "$release_directory/ssalddel-server.tar"

cp "$release_directory/compose.orderer-v15.override.yaml" "$override_file"
docker compose \
  --env-file "$environment_file" \
  -f "$compose_file" \
  -f "$override_file" \
  config --quiet

mkdir -p "$web_next"
tar -xzf "$release_directory/web.tar.gz" -C "$web_next"
test -f "$web_next/index.html"

mv "$application_root/web" "$web_backup"
mv "$web_next" "$application_root/web"

rollback() {
  if [[ -d "$web_backup" ]]; then
    rm -rf "$application_root/web"
    mv "$web_backup" "$application_root/web"
  fi
  docker tag "$old_image" ssalddel-server:azure-preview
  docker compose \
    --env-file "$environment_file" \
    -f "$compose_file" \
    up -d --no-deps --force-recreate app caddy
}

if ! docker compose \
  --env-file "$environment_file" \
  -f "$compose_file" \
  -f "$override_file" \
  up -d --no-deps --force-recreate app; then
  rollback
  exit 1
fi

healthy=false
for _ in $(seq 1 30); do
  status="$(docker inspect ssalddel-preview-app-1 --format '{{if .State.Health}}{{.State.Health.Status}}{{else}}{{.State.Status}}{{end}}')"
  if [[ "$status" == "healthy" ]]; then
    healthy=true
    break
  fi
  if [[ "$status" == "unhealthy" || "$status" == "exited" ]]; then
    break
  fi
  sleep 2
done

if [[ "$healthy" != "true" ]]; then
  docker logs --tail 100 ssalddel-preview-app-1 >&2 || true
  rollback
  exit 1
fi

docker compose \
  --env-file "$environment_file" \
  -f "$compose_file" \
  -f "$override_file" \
  up -d --no-deps --force-recreate caddy

printf 'rollback_image=%s\nweb_backup=%s\n' "$rollback_tag" "$web_backup"
