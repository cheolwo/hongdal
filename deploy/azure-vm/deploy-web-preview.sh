#!/usr/bin/env bash
set -euo pipefail

release_archive="${1:?web preview archive is required}"
expected_sha256="${2:?expected SHA-256 is required}"
application_root="${3:-/opt/ssalddel}"
caddyfile_source="${4:-}"
timestamp="$(date -u +%Y%m%dT%H%M%SZ)"

case "$application_root" in
  /opt/ssalddel) ;;
  *)
    echo "Application root must be /opt/ssalddel." >&2
    exit 1
    ;;
esac

for required_path in \
  "$release_archive" \
  "$application_root/web" \
  "$application_root/Caddyfile" \
  "$application_root/compose.yaml" \
  "$application_root/.env"; do
  if [[ ! -e "$required_path" ]]; then
    echo "Required deployment path is missing: $required_path" >&2
    exit 1
  fi
done

if [[ -n "$caddyfile_source" && ! -f "$caddyfile_source" ]]; then
  echo "Caddyfile source is missing: $caddyfile_source" >&2
  exit 1
fi

actual_sha256="$(sha256sum "$release_archive" | awk '{print $1}')"
if [[ "$actual_sha256" != "$expected_sha256" ]]; then
  echo "Web archive SHA-256 does not match." >&2
  exit 1
fi

web_next="$application_root/web-next-$timestamp"
web_backup="$application_root/web-before-$timestamp"
web_failed="$application_root/web-failed-$timestamp"
caddyfile_backup=""

mkdir -p "$web_next"
tar -xzf "$release_archive" -C "$web_next"
test -f "$web_next/index.html"
test -f "$web_next/preview-build.json"

mv "$application_root/web" "$web_backup"
mv "$web_next" "$application_root/web"

if [[ -n "$caddyfile_source" ]]; then
  caddyfile_backup="$application_root/Caddyfile-before-$timestamp"
  cp "$application_root/Caddyfile" "$caddyfile_backup"
  cp "$caddyfile_source" "$application_root/Caddyfile"
fi

recreate_caddy() {
  docker compose \
    --env-file "$application_root/.env" \
    -f "$application_root/compose.yaml" \
    up -d --no-deps --force-recreate caddy
}

rollback() {
  if [[ -d "$application_root/web" ]]; then
    mv "$application_root/web" "$web_failed"
  fi
  mv "$web_backup" "$application_root/web"
  if [[ -n "$caddyfile_backup" && -f "$caddyfile_backup" ]]; then
    cp "$caddyfile_backup" "$application_root/Caddyfile"
  fi
  recreate_caddy
}

if ! recreate_caddy; then
  rollback
  exit 1
fi

site_host="$(docker exec ssalddel-preview-caddy-1 printenv SSALDDEL_SITE_HOST)"
healthy=false
for _ in $(seq 1 15); do
  if curl \
      --fail \
      --silent \
      --show-error \
      --location \
      --max-time 20 \
      "https://$site_host/preview-build.json" >/dev/null; then
    healthy=true
    break
  fi
  sleep 2
done

if [[ "$healthy" != "true" ]]; then
  rollback
  exit 1
fi

printf 'web_backup=%s\ncaddyfile_backup=%s\nrelease_archive=%s\nsha256=%s\n' \
  "$web_backup" \
  "$caddyfile_backup" \
  "$release_archive" \
  "$actual_sha256"
