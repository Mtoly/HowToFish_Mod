#!/usr/bin/env bash
set -euo pipefail
BASELINE_DIR="${BASELINE_DIR:-$(dirname "$0")/task4-baseline-source}"
TARGET_DIR="${1:?usage: ROLLBACK_TASK4.sh TARGET_SOURCE_DIR}"
mkdir -p "$TARGET_DIR"
for name in ProjectileGuidance.cs CheatState.cs Presets.cs WeaponsTab.cs; do
  cp -- "$BASELINE_DIR/$name" "$TARGET_DIR/$name"
done
printf 'RESTORED_DIR=%s\n' "$TARGET_DIR"
printf 'ROLLBACK_MATCH=True\n'
