#!/usr/bin/env bash
set -euo pipefail
HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="${1:-/mnt/d/Code/How2fish/EnvinciblesMods-CompleteCheatMenu}"
GAME_DLL="${2:-/mnt/e/SteamLibrary/steamapps/common/How to Fish/How to Fish/BepInEx/plugins/EnvinciblesMods-CompleteCheatMenu/CompleteCheatMenu.dll}"
ORIGINAL_SOURCE="$HERE/original/source"
ORIGINAL_DLL="$HERE/original/game/CompleteCheatMenu.dll"
restore=(
 'decompiled-src/CompleteCheatMenu/Runtime/EspRendererV2.cs'
 'decompiled-src/CompleteCheatMenu/Runtime/CheatState.cs'
 'decompiled-src/CompleteCheatMenu/Runtime/Presets.cs'
 'decompiled-src/CompleteCheatMenu/UI/Tabs/VisualsTab.cs'
 'tests/localized_ui_preset_test.py'
 'docs/plans/2026-08-25-complete-cheat-menu-cnkx-port-implementation-plan.md'
)
for rel in "${restore[@]}"; do
 mkdir -p "$(dirname "$PROJECT_ROOT/$rel")"
 cp -f "$ORIGINAL_SOURCE/$rel" "$PROJECT_ROOT/$rel"
done
created=(
 'decompiled-src/CompleteCheatMenu/Targeting/AimOverlayMath.cs'
 'tests/aim_overlay_test.py'
 'tests/aim_overlay_harness.cs'
)
for rel in "${created[@]}"; do rm -f "$PROJECT_ROOT/$rel"; done
mkdir -p "$(dirname "$GAME_DLL")"
cp -f "$ORIGINAL_DLL" "$GAME_DLL"
expected='224a13ae043e203d3ebcbff840241bb32588a7549b8f8dc8326236513a4aab0a'
actual="$(sha256sum "$GAME_DLL" | awk '{print $1}')"
[[ "$actual" == "$expected" ]]
present=0
for rel in "${created[@]}"; do [[ -e "$PROJECT_ROOT/$rel" ]] && present=$((present+1)); done
printf 'ROLLBACK_TASK17_5=PASS\nRESTORED_DLL_SHA256=%s\nRESTORED_SOURCE_FILES=%s\nCREATED_FILES_PRESENT=%s\n' "${actual^^}" "${#restore[@]}" "$present"
