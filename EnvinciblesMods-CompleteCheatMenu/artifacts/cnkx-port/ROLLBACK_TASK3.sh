#!/usr/bin/env bash
set -euo pipefail
ROOT="${1:-/mnt/d/Code/How2fish/EnvinciblesMods-CompleteCheatMenu}"
SOURCE_ROOT="/mnt/d/Code/How2fish/EnvinciblesMods-CompleteCheatMenu/artifacts/cnkx-port/task3-original-src"
mkdir -p "$ROOT/decompiled-src/CompleteCheatMenu/Cheats" "$ROOT/decompiled-src/CompleteCheatMenu/Patches"
cp -- "$SOURCE_ROOT/AimTargetRegistry.cs" "$ROOT/decompiled-src/CompleteCheatMenu/Cheats/AimTargetRegistry.cs"
cp -- "$SOURCE_ROOT/CreatureRegistry_Patch.cs" "$ROOT/decompiled-src/CompleteCheatMenu/Patches/CreatureRegistry_Patch.cs"
created=(
  "decompiled-src/CompleteCheatMenu/Runtime/EntityRegistry.cs"
  "decompiled-src/CompleteCheatMenu/Runtime/EntityRecord.cs"
  "decompiled-src/CompleteCheatMenu/Patches/PlayerRegistry_Patch.cs"
  "tests/entity_registry_test.py"
)
for relative in "${created[@]}"; do
  [[ -e "$ROOT/$relative" ]] && rm -- "$ROOT/$relative"
done
aim="$(sha256sum "$ROOT/decompiled-src/CompleteCheatMenu/Cheats/AimTargetRegistry.cs" | awk '{print $1}')"
creature="$(sha256sum "$ROOT/decompiled-src/CompleteCheatMenu/Patches/CreatureRegistry_Patch.cs" | awk '{print $1}')"
remaining=0
for relative in "${created[@]}"; do
  [[ -e "$ROOT/$relative" ]] && remaining=$((remaining + 1))
done
printf 'AIM_REGISTRY_SHA256=%s\n' "${aim^^}"
printf 'CREATURE_PATCH_SHA256=%s\n' "${creature^^}"
printf 'CREATED_REMAINING=%d\n' "$remaining"
if [[ "$aim" == "e9d43aec1e04035c58f3e9e3f691fc7b7d00e98e0823e65d1a07bc74eaabf988" && "$creature" == "f6f139bf865bd944c957039528617e3619b7a5e3a341d07eb67f0a5e4697abeb" && "$remaining" -eq 0 ]]; then
  printf 'ROLLBACK_MATCH=True\n'
else
  printf 'ROLLBACK_MATCH=False\n'
  exit 2
fi
