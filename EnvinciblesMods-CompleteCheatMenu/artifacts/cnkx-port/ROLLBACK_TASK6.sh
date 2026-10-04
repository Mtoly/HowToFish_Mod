#!/usr/bin/env bash
set -euo pipefail
ROOT="${1:-/mnt/d/Code/How2fish/EnvinciblesMods-CompleteCheatMenu}"
SOURCE_ROOT="/mnt/d/Code/How2fish/EnvinciblesMods-CompleteCheatMenu/artifacts/cnkx-port/task6-original-src"
[[ -d "$ROOT" ]] || { printf 'DestinationRoot does not exist: %s\n' "$ROOT" >&2; exit 2; }
mkdir -p "$ROOT/decompiled-src"
cp -- "$SOURCE_ROOT/build.ps1" "$ROOT/decompiled-src/build.ps1"
created=(
  "decompiled-src/CompleteCheatMenu/Targeting/AimPoint.cs"
  "decompiled-src/CompleteCheatMenu/Targeting/BoneResolver.cs"
  "decompiled-src/CompleteCheatMenu/Targeting/RendererBoundsResolver.cs"
  "tests/cnkx_bone_impl.py"
  "tests/bone_selection_test.py"
)
for relative in "${created[@]}"; do
  [[ -e "$ROOT/$relative" ]] && rm -- "$ROOT/$relative"
done
build_hash="$(sha256sum "$ROOT/decompiled-src/build.ps1" | awk '{print $1}')"
remaining=0
for relative in "${created[@]}"; do
  [[ -e "$ROOT/$relative" ]] && remaining=$((remaining + 1))
done
printf 'BUILD_PS1_SHA256=%s\n' "${build_hash^^}"
printf 'CREATED_REMAINING=%d\n' "$remaining"
if [[ "$build_hash" == "894da10d6af69655bcc47aaf900ccb479b3577d1502de63eadf10d8d91087253" && "$remaining" -eq 0 ]]; then
  printf 'ROLLBACK_MATCH=True\n'
else
  printf 'ROLLBACK_MATCH=False\n'
  exit 2
fi
