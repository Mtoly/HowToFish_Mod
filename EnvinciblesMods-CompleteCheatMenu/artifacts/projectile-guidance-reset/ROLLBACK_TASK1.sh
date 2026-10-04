#!/usr/bin/env bash
set -euo pipefail
BASELINE="${BASELINE:-$(dirname "$0")/projectile_guidance_reset_test.baseline.py}"
DESTINATION="${1:?usage: ROLLBACK_TASK1.sh DESTINATION}"
mkdir -p "$(dirname "$DESTINATION")"
cp -- "$BASELINE" "$DESTINATION"
actual="$(sha256sum "$DESTINATION" | awk '{print toupper($1)}')"
expected="$(sha256sum "$BASELINE" | awk '{print toupper($1)}')"
printf 'RESTORED_FILE=%s\n' "$DESTINATION"
printf 'RESTORED_SHA256=%s\n' "$actual"
printf 'EXPECTED_SHA256=%s\n' "$expected"
[[ "$actual" == "$expected" ]]
printf 'ROLLBACK_MATCH=True\n'
