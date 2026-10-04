#!/usr/bin/env bash
set -euo pipefail
ROOT="${1:-/mnt/d/Code/How2fish/EnvinciblesMods-CompleteCheatMenu}"
SOURCE_ROOT="/mnt/d/Code/How2fish/EnvinciblesMods-CompleteCheatMenu/artifacts/cnkx-port/task11-original-src"
[[ -d "$ROOT" ]] || { printf 'DestinationRoot does not exist: %s\n' "$ROOT" >&2; exit 2; }
mkdir -p "$ROOT/decompiled-src/CompleteCheatMenu/Cheats" "$ROOT/decompiled-src/CompleteCheatMenu/Runtime" "$ROOT/tests"
cp -- "$SOURCE_ROOT/CheatState.cs" "$ROOT/decompiled-src/CompleteCheatMenu/Runtime/CheatState.cs"
cp -- "$SOURCE_ROOT/Plugin.cs" "$ROOT/decompiled-src/CompleteCheatMenu/Plugin.cs"
cp -- "$SOURCE_ROOT/WeaponCheats.cs" "$ROOT/decompiled-src/CompleteCheatMenu/Cheats/WeaponCheats.cs"
created=(
  "decompiled-src/CompleteCheatMenu/Cheats/WeaponStateStore.cs"
  "tests/cnkx_weapon_store_impl.py"
  "tests/weapon_restore_test.py"
)
for relative in "${created[@]}"; do
  [[ -e "$ROOT/$relative" ]] && rm -- "$ROOT/$relative"
done
cheat="$(sha256sum "$ROOT/decompiled-src/CompleteCheatMenu/Runtime/CheatState.cs" | awk '{print $1}')"
plugin="$(sha256sum "$ROOT/decompiled-src/CompleteCheatMenu/Plugin.cs" | awk '{print $1}')"
weapon="$(sha256sum "$ROOT/decompiled-src/CompleteCheatMenu/Cheats/WeaponCheats.cs" | awk '{print $1}')"
remaining=0
for relative in "${created[@]}"; do
  [[ -e "$ROOT/$relative" ]] && remaining=$((remaining + 1))
done
printf 'CHEATSTATE_SHA256=%s\n' "${cheat^^}"
printf 'PLUGIN_SHA256=%s\n' "${plugin^^}"
printf 'WEAPON_SHA256=%s\n' "${weapon^^}"
printf 'CREATED_REMAINING=%d\n' "$remaining"
if [[ "$cheat" == "11bc1b747042bd019fc1b5126084ffb5d00cee4770c093bf21e4fe97176cb5d5" && "$plugin" == "86a84f114a2e0ac657f0b53d9354c97d6652f9d6310061f0eeead9705bd8fbfc" && "$weapon" == "c5e2a322c2b0bf65a57e52c44f8d09a574936df026d78480858cdc4db868732f" && "$remaining" -eq 0 ]]; then
  printf 'ROLLBACK_MATCH=True\n'
else
  printf 'ROLLBACK_MATCH=False\n'
  exit 2
fi
