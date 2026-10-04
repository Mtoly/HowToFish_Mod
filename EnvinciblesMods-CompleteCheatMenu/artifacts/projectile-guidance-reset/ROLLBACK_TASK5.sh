#!/usr/bin/env bash
set -euo pipefail
BASELINE_DIR="${BASELINE_DIR:-$(dirname "$0")/task5-baseline-source}"
TARGET_DIR="${1:?usage: ROLLBACK_TASK5.sh TARGET_SOURCE_DIR}"
mkdir -p "$TARGET_DIR/Targeting" "$TARGET_DIR/Patches"
cp -- "$BASELINE_DIR/MagicShotContext.cs" "$TARGET_DIR/Targeting/MagicShotContext.cs"
cp -- "$BASELINE_DIR/WeaponShoot_Patch.cs" "$TARGET_DIR/Patches/WeaponShoot_Patch.cs"
printf 'RESTORED_DIR=%s\nROLLBACK_MATCH=True\n' "$TARGET_DIR"
