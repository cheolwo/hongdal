#!/usr/bin/env bash
set -euo pipefail

release_archive="${1:?release archive is required}"
expected_sha256="${2:?expected SHA-256 is required}"
environment_file="${3:?environment file is required}"
application_root="${4:-/opt/ssalddel-unity-review}"
timestamp="$(date -u +%Y%m%dT%H%M%SZ)"

if [[ "$application_root" != "/opt/ssalddel-unity-review" ]]; then
  echo "Application root must be /opt/ssalddel-unity-review." >&2
  exit 1
fi
for path in "$release_archive" "$environment_file"; do
  if [[ ! -f "$path" ]]; then
    echo "Required deployment file is missing: $path" >&2
    exit 1
  fi
done

actual_sha256="$(sha256sum "$release_archive" | awk '{print $1}')"
if [[ "$actual_sha256" != "$expected_sha256" ]]; then
  echo "Unity Review release SHA-256 does not match." >&2
  exit 1
fi

release_root="$application_root/releases/$timestamp"
previous_target=""
if [[ -L "$application_root/current" ]]; then
  previous_target="$(readlink -f "$application_root/current")"
fi
install -d -m 0750 "$release_root"
tar -xzf "$release_archive" -C "$release_root"
test -f "$release_root/api/Ssalddel.UnityReview.Api.dll"
test -f "$release_root/web/index.html"
test -f "$release_root/compose.yaml"
test -f "$release_root/Caddyfile"
install -m 0600 "$environment_file" "$application_root/.env"
ln -sfn "$release_root" "$application_root/current-next"
mv -Tf "$application_root/current-next" "$application_root/current"

start_release() {
  local target="$1"
  docker compose \
    --env-file "$application_root/.env" \
    -f "$target/compose.yaml" \
    up -d --pull always --remove-orphans
}

rollback() {
  if [[ -n "$previous_target" && -d "$previous_target" ]]; then
    ln -sfn "$previous_target" "$application_root/current-next"
    mv -Tf "$application_root/current-next" "$application_root/current"
    start_release "$previous_target"
  fi
}

if ! start_release "$release_root"; then
  rollback
  exit 1
fi

site_host="$(sed -n 's/^UNITY_REVIEW_SITE_HOST=//p' "$application_root/.env" | head -n 1)"
healthy=false
for _ in $(seq 1 30); do
  if curl --fail --silent --show-error --max-time 15 "https://$site_host/healthz" >/dev/null \
    && curl --fail --silent --show-error --max-time 15 "https://$site_host/preview-build.json" >/dev/null; then
    healthy=true
    break
  fi
  sleep 4
done
if [[ "$healthy" != "true" ]]; then
  docker compose --env-file "$application_root/.env" -f "$release_root/compose.yaml" logs --tail 120
  rollback
  exit 1
fi

printf 'release=%s\nsha256=%s\nsite=https://%s/\n' "$release_root" "$actual_sha256" "$site_host"
