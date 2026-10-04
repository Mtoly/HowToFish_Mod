#!/usr/bin/env bash
set -euo pipefail
MODIFIED_FILE="${1:?modified file}"
BASELINE_FILE="${2:?baseline file}"
cp -- "$BASELINE_FILE" "$MODIFIED_FILE"
actual=$(sha256sum "$MODIFIED_FILE" | awk '{print toupper($1)}')
expected=$(sha256sum "$BASELINE_FILE" | awk '{print toupper($1)}')
test "$actual" = "$expected"
printf 'ROLLBACK_SH_VERIFICATION=PASS\nRESTORED_DLL_SHA256=%s\n' "$actual"
