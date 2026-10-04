#!/usr/bin/env bash
set -euo pipefail
ROOT="${1:-/mnt/d/Code/How2fish/EnvinciblesMods-CompleteCheatMenu}"
SOURCE_ROOT="/mnt/d/Code/How2fish/EnvinciblesMods-CompleteCheatMenu/artifacts/cnkx-port/task4-original-src"
mkdir -p "$ROOT/decompiled-src/CompleteCheatMenu/Cheats" "$ROOT/decompiled-src/CompleteCheatMenu/Runtime"
cp -- "$SOURCE_ROOT/VisualCheats.cs" "$ROOT/decompiled-src/CompleteCheatMenu/Cheats/VisualCheats.cs"
cp -- "$SOURCE_ROOT/TickDriver.cs" "$ROOT/decompiled-src/CompleteCheatMenu/Runtime/TickDriver.cs"
cp -- "$SOURCE_ROOT/WeaponCheats.cs" "$ROOT/decompiled-src/CompleteCheatMenu/Cheats/WeaponCheats.cs"
created=(
  "decompiled-src/CompleteCheatMenu/Runtime/EspSnapshot.cs"
  "decompiled-src/CompleteCheatMenu/Runtime/EspSnapshotBuilder.cs"
  "decompiled-src/CompleteCheatMenu/Targeting/TargetingSnapshot.cs"
  "tests/snapshot_isolation_test.py"
)
for relative in "${created[@]}"; do
  [[ -e "$ROOT/$relative" ]] && rm -- "$ROOT/$relative"
done
visual="$(sha256sum "$ROOT/decompiled-src/CompleteCheatMenu/Cheats/VisualCheats.cs" | awk '{print $1}')"
tick="$(sha256sum "$ROOT/decompiled-src/CompleteCheatMenu/Runtime/TickDriver.cs" | awk '{print $1}')"
weapon="$(sha256sum "$ROOT/decompiled-src/CompleteCheatMenu/Cheats/WeaponCheats.cs" | awk '{print $1}')"
remaining=0
for relative in "${created[@]}"; do
  [[ -e "$ROOT/$relative" ]] && remaining=$((remaining + 1))
done
printf 'VISUAL_SHA256=%s\n' "${visual^^}"
printf 'TICK_SHA256=%s\n' "${tick^^}"
printf 'WEAPON_SHA256=%s\n' "${weapon^^}"
printf 'CREATED_REMAINING=%d\n' "$remaining"
if [[ "$visual" == "8eb9250ec1a39203f45078a8baf743a59647c3ae35828b5e38db971118dd3556" && "$tick" == "2aa41cc3f5de2c4aa6111eefabeb6d0a7e980362b2b04ca50e278b4d305c394c" && "$weapon" == "d9ff172f97f8a4cafb4ea2fff7e2e36033191994028dfa83b61dad49440d3916" && "$remaining" -eq 0 ]]; then
  printf 'ROLLBACK_MATCH=True\n'
else
  printf 'ROLLBACK_MATCH=False\n'
  exit 2
fi
