#!/usr/bin/env bash
set -euo pipefail
ROOT="${1:?usage: ROLLBACK_TASK17_4.sh DESTINATION_ROOT [DESTINATION_DLL] [BASELINE_DLL]}";DEST_DLL="${2:-}";BASE_DLL="${3:-D:/Code/How2fish/EnvinciblesMods-CompleteCheatMenu/artifacts/cnkx-port/task17-3-initial-velocity/CompleteCheatMenu.task17-3.zh-CN.dll}";SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)";ORIGINAL="$SCRIPT_DIR/original"
restore(){ mkdir -p "$(dirname "$ROOT/$2")";cp -f "$ORIGINAL/$1" "$ROOT/$2"; }
restore decompiled-src__CompleteCheatMenu__Runtime__CheatState.cs decompiled-src/CompleteCheatMenu/Runtime/CheatState.cs
restore decompiled-src__CompleteCheatMenu__Runtime__Presets.cs decompiled-src/CompleteCheatMenu/Runtime/Presets.cs
restore decompiled-src__CompleteCheatMenu__UI__Tabs__WeaponsTab.cs decompiled-src/CompleteCheatMenu/UI/Tabs/WeaponsTab.cs
restore decompiled-src__CompleteCheatMenu__UI__Tabs__DiagnosticsTab.cs decompiled-src/CompleteCheatMenu/UI/Tabs/DiagnosticsTab.cs
restore decompiled-src__CompleteCheatMenu__Plugin.cs decompiled-src/CompleteCheatMenu/Plugin.cs
restore decompiled-src__CompleteCheatMenu__Targeting__BallisticPredictor.cs decompiled-src/CompleteCheatMenu/Targeting/BallisticPredictor.cs
restore decompiled-src__CompleteCheatMenu__Targeting__InitialVelocityRedirector.cs decompiled-src/CompleteCheatMenu/Targeting/InitialVelocityRedirector.cs
restore decompiled-src__CompleteCheatMenu__Patches__WeaponAddProjectile_Patch.cs decompiled-src/CompleteCheatMenu/Patches/WeaponAddProjectile_Patch.cs
restore decompiled-src__CompleteCheatMenu__Patches__WeaponAddProjectiles_Patch.cs decompiled-src/CompleteCheatMenu/Patches/WeaponAddProjectiles_Patch.cs
restore tests__ballistic_prediction_test.py tests/ballistic_prediction_test.py
restore tests__projectile_initial_velocity_harness.cs tests/projectile_initial_velocity_harness.cs
restore docs__plans__2026-08-25-complete-cheat-menu-cnkx-port-implementation-plan.md docs/plans/2026-08-25-complete-cheat-menu-cnkx-port-implementation-plan.md
rm -f "$ROOT/decompiled-src/CompleteCheatMenu/Targeting/ProjectileGuidance.cs" "$ROOT/decompiled-src/CompleteCheatMenu/Targeting/ProjectileGuidanceMath.cs" "$ROOT/decompiled-src/CompleteCheatMenu/Patches/ProjectileUpdateScan_Patch.cs" "$ROOT/decompiled-src/CompleteCheatMenu/Patches/ProjectileRemove_Patch.cs" "$ROOT/tests/projectile_guidance_test.py" "$ROOT/tests/projectile_guidance_harness.cs" "$ROOT/tests/projectile_guidance_lifecycle_harness.cs"
if [[ -n "$DEST_DLL" ]];then mkdir -p "$(dirname "$DEST_DLL")";cp -f "$BASE_DLL" "$DEST_DLL";fi
printf 'RESTORED_SOURCE_FILES=12\nCREATED_FILES_PRESENT=0\nROLLBACK_TASK17_4=PASS\n'
