#!/usr/bin/env bash
set -euo pipefail
SOURCE="/mnt/d/Code/How2fish/EnvinciblesMods-CompleteCheatMenu/artifacts/cnkx-port/original-src/GameBinder.cs"
DESTINATION="${1:-/mnt/d/Code/How2fish/EnvinciblesMods-CompleteCheatMenu/decompiled-src/CompleteCheatMenu/Game/GameBinder.cs}"
EXPECTED="161300e1a8a5aad437171b43a4fef012222d86748c09278efa6df2f4bbaccdac"
cp -- "$SOURCE" "$DESTINATION"
ACTUAL="$(sha256sum "$DESTINATION" | awk '{print $1}')"
ACTUAL_LOWER="${ACTUAL,,}"
printf 'ROLLBACK_SHA256=%s\n' "${ACTUAL_LOWER^^}"
if [[ "$ACTUAL_LOWER" == "$EXPECTED" ]]; then
  printf 'ROLLBACK_MATCH=True\n'
else
  printf 'ROLLBACK_MATCH=False\n'
  exit 2
fi
