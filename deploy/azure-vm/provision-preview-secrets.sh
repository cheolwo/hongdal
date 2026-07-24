#!/usr/bin/env bash
set -euo pipefail

environment_file="${1:-/opt/ssalddel/.env}"
environment_directory="$(dirname "$environment_file")"
timestamp="$(date -u +%Y%m%dT%H%M%SZ)"

if [[ ! -f "$environment_file" ]]; then
  echo "Environment file was not found: $environment_file" >&2
  exit 1
fi

if grep -q '^SSALDDEL_ISMS_P_AES_KEY_BASE64=.' "$environment_file" \
  && grep -q '^SSALDDEL_ISMS_P_HASH_SALT=.' "$environment_file"; then
  chmod 600 "$environment_file"
  echo "Protected-data secrets are already configured."
  exit 0
fi

cp "$environment_file" "$environment_directory/.env.before-ismp-$timestamp"
chmod 600 "$environment_directory/.env.before-ismp-$timestamp"

aes_key="$(openssl rand -base64 32 | tr -d '\n')"
hash_salt="$(openssl rand -hex 32)"
printf '\nSSALDDEL_ISMS_P_AES_KEY_BASE64=%s\nSSALDDEL_ISMS_P_HASH_SALT=%s\n' \
  "$aes_key" \
  "$hash_salt" \
  >> "$environment_file"
chmod 600 "$environment_file"

echo "Protected-data secrets were generated without printing their values."
