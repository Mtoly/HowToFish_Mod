#!/usr/bin/env bash
set -euo pipefail
BASELINE="${BASELINE:-$(dirname "$0")/baseline-plan.md}"
DESTINATION="${1:?usage: ROLLBACK.sh DESTINATION}"
mkdir -p "$(dirname "$DESTINATION")"
cp -- "$BASELINE" "$DESTINATION"
actual="$(sha256sum "$DESTINATION" | awk '{print toupper($1)}')"
expected="$(sha256sum "$BASELINE" | awk '{print toupper($1)}')"
printf 'RESTORED_FILE=%s\n' "$DESTINATION"
printf 'RESTORED_SHA256=%s\n' "$actual"
printf 'EXPECTED_SHA256=%s\n' "$expected"
if [[ "$actual" != "$expected" ]]; then
  printf 'ROLLBACK_MATCH=False\n'
  exit 2
fi
printf 'ROLLBACK_MATCH=True\n'
