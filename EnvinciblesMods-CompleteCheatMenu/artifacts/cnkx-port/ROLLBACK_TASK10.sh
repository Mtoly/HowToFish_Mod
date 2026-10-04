#!/usr/bin/env bash
set -euo pipefail
ROOT="${1:-/mnt/d/Code/How2fish/EnvinciblesMods-CompleteCheatMenu}"
SOURCE_ROOT="/mnt/d/Code/How2fish/EnvinciblesMods-CompleteCheatMenu/artifacts/cnkx-port/task10-original-src"
[[ -d "$ROOT" ]] || { printf 'DestinationRoot does not exist: %s\n' "$ROOT" >&2; exit 2; }
mkdir -p "$ROOT/decompiled-src/CompleteCheatMenu/Cheats" "$ROOT/decompiled-src/CompleteCheatMenu/Runtime" "$ROOT/tests"
cp -- "$SOURCE_ROOT/WeaponCheats.cs" "$ROOT/decompiled-src/CompleteCheatMenu/Cheats/WeaponCheats.cs"
cp -- "$SOURCE_ROOT/CheatState.cs" "$ROOT/decompiled-src/CompleteCheatMenu/Runtime/CheatState.cs"
cp -- "$SOURCE_ROOT/TickDriver.cs" "$ROOT/decompiled-src/CompleteCheatMenu/Runtime/TickDriver.cs"
created=(
  "decompiled-src/CompleteCheatMenu/Targeting/VisibleAimController.cs"
  "tests/cnkx_visible_aim_impl.py"
  "tests/visible_aim_test.py"
)
for relative in "${created[@]}"; do
  [[ -e "$ROOT/$relative" ]] && rm -- "$ROOT/$relative"
done
weapon="$(sha256sum "$ROOT/decompiled-src/CompleteCheatMenu/Cheats/WeaponCheats.cs" | awk '{print $1}')"
cheat="$(sha256sum "$ROOT/decompiled-src/CompleteCheatMenu/Runtime/CheatState.cs" | awk '{print $1}')"
tick="$(sha256sum "$ROOT/decompiled-src/CompleteCheatMenu/Runtime/TickDriver.cs" | awk '{print $1}')"
remaining=0
for relative in "${created[@]}"; do
  [[ -e "$ROOT/$relative" ]] && remaining=$((remaining + 1))
done
printf 'WEAPON_SHA256=%s\n' "${weapon^^}"
printf 'CHEATSTATE_SHA256=%s\n' "${cheat^^}"
printf 'TICK_DRIVER_SHA256=%s\n' "${tick^^}"
printf 'CREATED_REMAINING=%d\n' "$remaining"
if [[ "$weapon" == "a7e0e2256cfee9709246f084b129ef6c62313a545dc201e24604eb33bcabb051" && "$cheat" == "9a118e2e4d033eab6d1fd91b5c9109d7c6d77e96421afa6f7eb1051c08e71df9" && "$tick" == "c0e7760c49980b50eaa316dca0bcee88881da47cca7e0a6f4480e56ce7818af4" && "$remaining" -eq 0 ]]; then
  printf 'ROLLBACK_MATCH=True\n'
else
  printf 'ROLLBACK_MATCH=False\n'
  exit 2
fi
