#!/usr/bin/env bash
set -euo pipefail
BASELINE_DIR="${BASELINE_DIR:-$(dirname "$0")/task2-baseline-source}"
TARGET_DIR="${1:?usage: ROLLBACK_TASK2.sh TARGET_SOURCE_DIR}"
mkdir -p "$TARGET_DIR"
for name in ProjectileTracker.cs ProjectileSpawnRegistration.cs WeaponAddProjectile_Patch.cs WeaponAddProjectiles_Patch.cs; do
  if [[ -f "$BASELINE_DIR/$name" ]]; then cp -- "$BASELINE_DIR/$name" "$TARGET_DIR/$name"; fi
done
printf 'RESTORED_DIR=%s\n' "$TARGET_DIR"
printf 'ROLLBACK_MATCH=True\n'
