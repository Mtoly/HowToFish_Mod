#!/usr/bin/env bash
set -euo pipefail

ROOT="${1:?usage: ROLLBACK_TASK17_1.sh DESTINATION_ROOT [DESTINATION_DLL] [BASELINE_DLL]}"
DEST_DLL="${2:-}"
BASE_DLL="${3:-D:/Code/How2fish/EnvinciblesMods-CompleteCheatMenu/CompleteCheatMenu.client-mode.zh-CN.dll}"
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ORIGINAL="$SCRIPT_DIR/original"

restore() {
  local backup="$1"
  local relative="$2"
  mkdir -p "$(dirname "$ROOT/$relative")"
  cp -f "$ORIGINAL/$backup" "$ROOT/$relative"
}

restore decompiled-src__CompleteCheatMenu__Targeting__TargetingSystem.cs decompiled-src/CompleteCheatMenu/Targeting/TargetingSystem.cs
restore decompiled-src__CompleteCheatMenu__Targeting__VisibilityCache.cs decompiled-src/CompleteCheatMenu/Targeting/VisibilityCache.cs
restore decompiled-src__CompleteCheatMenu__Runtime__EspRendererV2.cs decompiled-src/CompleteCheatMenu/Runtime/EspRendererV2.cs
restore decompiled-src__CompleteCheatMenu__UI__Tabs__WeaponsTab.cs decompiled-src/CompleteCheatMenu/UI/Tabs/WeaponsTab.cs
restore tests__esp_projection_test.py tests/esp_projection_test.py
restore docs__plans__2026-08-25-complete-cheat-menu-cnkx-port-implementation-plan.md docs/plans/2026-08-25-complete-cheat-menu-cnkx-port-implementation-plan.md
rm -f "$ROOT/tests/visibility_ui_semantics_test.py"

if [[ -n "$DEST_DLL" ]]; then
  mkdir -p "$(dirname "$DEST_DLL")"
  cp -f "$BASE_DLL" "$DEST_DLL"
fi

printf 'RESTORED_SOURCE_FILES=6\n'
printf 'CREATED_TEST_PRESENT=false\n'
printf 'ROLLBACK_TASK17_1=PASS\n'
