#!/usr/bin/env bash
set -euo pipefail

release_archive="${1:?Unity Review WebApp archive is required}"
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
  echo "Unity Review WebApp archive SHA-256 does not match." >&2
  exit 1
fi

review_next="$application_root/unity-review-next-$timestamp"
review_target="$application_root/web/unity-review"
review_backup="$application_root/unity-review-before-$timestamp"
review_failed="$application_root/unity-review-failed-$timestamp"
caddyfile_backup=""

mkdir -p "$review_next"
tar -xzf "$release_archive" -C "$review_next"
test -f "$review_next/index.html"
test -f "$review_next/preview-build.json"
grep -F '<base href="/unity-review/" />' "$review_next/index.html" >/dev/null

if [[ -d "$review_target" ]]; then
  mv "$review_target" "$review_backup"
fi
mv "$review_next" "$review_target"

if [[ -n "$caddyfile_source" ]]; then
  caddyfile_backup="$application_root/Caddyfile-before-unity-review-$timestamp"
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
  if [[ -d "$review_target" ]]; then
    mv "$review_target" "$review_failed"
  fi
  if [[ -d "$review_backup" ]]; then
    mv "$review_backup" "$review_target"
  fi
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
      "https://$site_host/unity-review/preview-build.json" >/dev/null \
    && curl \
      --fail \
      --silent \
      --show-error \
      --location \
      --max-time 20 \
      "https://$site_host/unity-review/" >/dev/null; then
    healthy=true
    break
  fi
  sleep 2
done

if [[ "$healthy" != "true" ]]; then
  rollback
  exit 1
fi

printf 'review_backup=%s\ncaddyfile_backup=%s\nrelease_archive=%s\nsha256=%s\n' \
  "$review_backup" \
  "$caddyfile_backup" \
  "$release_archive" \
  "$actual_sha256"
