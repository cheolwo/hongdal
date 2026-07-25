#!/usr/bin/env bash
set -euo pipefail

release_directory="${1:?release directory is required}"
application_root="${2:-/opt/ssalddel}"
script_directory="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"

exec bash "$script_directory/deploy-preview-profile.sh" \
  "$release_directory" \
  mart-v35 \
  "$application_root"
