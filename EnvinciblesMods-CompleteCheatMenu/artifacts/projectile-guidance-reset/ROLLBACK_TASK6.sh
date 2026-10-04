#!/usr/bin/env bash
set -euo pipefail
BASELINE_DLL="${BASELINE_DLL:-$(dirname "$0")/CompleteCheatMenu.task5.dll}"
TARGET_DIR="${1:?usage: ROLLBACK_TASK6.sh TARGET_DIR}"
mkdir -p "$TARGET_DIR"
cp -- "$BASELINE_DLL" "$TARGET_DIR/CompleteCheatMenu.dll"
printf 'RESTORED_DIR=%s\nROLLBACK_MATCH=True\n' "$TARGET_DIR"
