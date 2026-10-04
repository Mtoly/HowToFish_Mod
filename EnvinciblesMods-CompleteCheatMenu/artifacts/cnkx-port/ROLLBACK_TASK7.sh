#!/usr/bin/env bash
set -euo pipefail
ROOT="${1:-/mnt/d/Code/How2fish/EnvinciblesMods-CompleteCheatMenu}"
SOURCE_ROOT="/mnt/d/Code/How2fish/EnvinciblesMods-CompleteCheatMenu/artifacts/cnkx-port/task7-original-src"
[[ -d "$ROOT" ]] || { printf 'DestinationRoot does not exist: %s\n' "$ROOT" >&2; exit 2; }
mkdir -p "$ROOT/decompiled-src/CompleteCheatMenu/Cheats" "$ROOT/decompiled-src/CompleteCheatMenu/UI/Tabs" "$ROOT/tests"
cp -- "$SOURCE_ROOT/WeaponCheats.cs" "$ROOT/decompiled-src/CompleteCheatMenu/Cheats/WeaponCheats.cs"
cp -- "$SOURCE_ROOT/WeaponsTab.cs" "$ROOT/decompiled-src/CompleteCheatMenu/UI/Tabs/WeaponsTab.cs"
cp -- "$SOURCE_ROOT/snapshot_isolation_test.py" "$ROOT/tests/snapshot_isolation_test.py"
created=(
  "decompiled-src/CompleteCheatMenu/Targeting/VisibilityCache.cs"
  "decompiled-src/CompleteCheatMenu/Targeting/TargetingSystem.cs"
  "tests/cnkx_target_lock_impl.py"
  "tests/target_lock_test.py"
)
for relative in "${created[@]}"; do
  [[ -e "$ROOT/$relative" ]] && rm -- "$ROOT/$relative"
done
weapon="$(sha256sum "$ROOT/decompiled-src/CompleteCheatMenu/Cheats/WeaponCheats.cs" | awk '{print $1}')"
tab="$(sha256sum "$ROOT/decompiled-src/CompleteCheatMenu/UI/Tabs/WeaponsTab.cs" | awk '{print $1}')"
snapshot="$(sha256sum "$ROOT/tests/snapshot_isolation_test.py" | awk '{print $1}')"
remaining=0
for relative in "${created[@]}"; do
  [[ -e "$ROOT/$relative" ]] && remaining=$((remaining + 1))
done
printf 'WEAPON_SHA256=%s\n' "${weapon^^}"
printf 'WEAPONS_TAB_SHA256=%s\n' "${tab^^}"
printf 'SNAPSHOT_TEST_SHA256=%s\n' "${snapshot^^}"
printf 'CREATED_REMAINING=%d\n' "$remaining"
if [[ "$weapon" == "77f5bc19fe6d7f5ecc0e37f56728fca811c58a98bae675282c3f63798527274d" && "$tab" == "3ac916ed304208b69c3ceee0927e38779c04787be2dfab926249d609e7991271" && "$snapshot" == "6916348c59ceaee39af143ef3fc113493ffa9ea6dfda8c40736352054978201f" && "$remaining" -eq 0 ]]; then
  printf 'ROLLBACK_MATCH=True\n'
else
  printf 'ROLLBACK_MATCH=False\n'
  exit 2
fi
