#!/usr/bin/env bash
set -euo pipefail
ROOT="${1:?usage: ROLLBACK_TASK17_2.sh DESTINATION_ROOT [DESTINATION_DLL] [BASELINE_DLL]}"
DEST_DLL="${2:-}"
BASE_DLL="${3:-D:/Code/How2fish/EnvinciblesMods-CompleteCheatMenu/artifacts/cnkx-port/task17-1-visibility-ui/CompleteCheatMenu.task17-1.zh-CN.dll}"
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ORIGINAL="$SCRIPT_DIR/original"
restore(){ mkdir -p "$(dirname "$ROOT/$2")"; cp -f "$ORIGINAL/$1" "$ROOT/$2"; }
restore decompiled-src__CompleteCheatMenu__Plugin.cs decompiled-src/CompleteCheatMenu/Plugin.cs
restore decompiled-src__CompleteCheatMenu__Runtime__TickDriver.cs decompiled-src/CompleteCheatMenu/Runtime/TickDriver.cs
restore decompiled-src__CompleteCheatMenu__UI__Tabs__DiagnosticsTab.cs decompiled-src/CompleteCheatMenu/UI/Tabs/DiagnosticsTab.cs
restore docs__plans__2026-08-25-complete-cheat-menu-cnkx-port-implementation-plan.md docs/plans/2026-08-25-complete-cheat-menu-cnkx-port-implementation-plan.md
rm -f "$ROOT/decompiled-src/CompleteCheatMenu/Targeting/ProjectileBindings.cs" "$ROOT/decompiled-src/CompleteCheatMenu/Targeting/ProjectileOwnership.cs" "$ROOT/decompiled-src/CompleteCheatMenu/Targeting/ProjectileTracker.cs" "$ROOT/decompiled-src/CompleteCheatMenu/Targeting/MagicShotContext.cs" "$ROOT/tests/projectile_lifecycle_test.py" "$ROOT/tests/projectile_lifecycle_harness.cs"
if [[ -n "$DEST_DLL" ]]; then mkdir -p "$(dirname "$DEST_DLL")"; cp -f "$BASE_DLL" "$DEST_DLL"; fi
printf 'RESTORED_SOURCE_FILES=4\nCREATED_FILES_PRESENT=0\nROLLBACK_TASK17_2=PASS\n'
