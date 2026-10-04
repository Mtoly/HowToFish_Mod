#!/usr/bin/env bash
set -euo pipefail
BASELINE_DIR="${BASELINE_DIR:-$(dirname "$0")/original-source}"
TARGET_ROOT="${1:?usage: ROLLBACK_MAGICBULLET_STYLE.sh TARGET_ROOT}"
mkdir -p "$TARGET_ROOT/Cheats" "$TARGET_ROOT/Targeting"
cp -- "$BASELINE_DIR/WeaponCheats.cs" "$TARGET_ROOT/Cheats/WeaponCheats.cs"
cp -- "$BASELINE_DIR/ShotRedirector.cs" "$TARGET_ROOT/Targeting/ShotRedirector.cs"
printf 'RESTORED_ROOT=%s\nROLLBACK_MATCH=True\n' "$TARGET_ROOT"
