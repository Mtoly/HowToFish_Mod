#!/usr/bin/env bash
set -euo pipefail
ROOT="${1:?usage: ROLLBACK_TASK17_3.sh DESTINATION_ROOT [DESTINATION_DLL] [BASELINE_DLL]}"
DEST_DLL="${2:-}"
BASE_DLL="${3:-D:/Code/How2fish/EnvinciblesMods-CompleteCheatMenu/artifacts/cnkx-port/task17-2-projectile-lifecycle/CompleteCheatMenu.task17-2.zh-CN.dll}"
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"; ORIGINAL="$SCRIPT_DIR/original"
restore(){ mkdir -p "$(dirname "$ROOT/$2")"; cp -f "$ORIGINAL/$1" "$ROOT/$2"; }
restore decompiled-src__CompleteCheatMenu__Patches__WeaponShoot_Patch.cs decompiled-src/CompleteCheatMenu/Patches/WeaponShoot_Patch.cs
restore decompiled-src__CompleteCheatMenu__Cheats__WeaponCheats.cs decompiled-src/CompleteCheatMenu/Cheats/WeaponCheats.cs
restore decompiled-src__CompleteCheatMenu__Targeting__ProjectileBindings.cs decompiled-src/CompleteCheatMenu/Targeting/ProjectileBindings.cs
restore decompiled-src__CompleteCheatMenu__Targeting__MagicShotContext.cs decompiled-src/CompleteCheatMenu/Targeting/MagicShotContext.cs
restore tests__tracking_probability_test.py tests/tracking_probability_test.py
restore tests__projectile_lifecycle_harness.cs tests/projectile_lifecycle_harness.cs
restore docs__plans__2026-08-25-complete-cheat-menu-cnkx-port-implementation-plan.md docs/plans/2026-08-25-complete-cheat-menu-cnkx-port-implementation-plan.md
rm -f "$ROOT/decompiled-src/CompleteCheatMenu/Targeting/InitialVelocityRedirector.cs" "$ROOT/decompiled-src/CompleteCheatMenu/Targeting/ProjectileSpawnRegistration.cs" "$ROOT/decompiled-src/CompleteCheatMenu/Patches/WeaponAddProjectile_Patch.cs" "$ROOT/decompiled-src/CompleteCheatMenu/Patches/WeaponAddProjectiles_Patch.cs" "$ROOT/tests/projectile_initial_velocity_test.py" "$ROOT/tests/projectile_initial_velocity_harness.cs" "$ROOT/tests/projectile_spawn_registration_harness.cs"
if [[ -n "$DEST_DLL" ]]; then mkdir -p "$(dirname "$DEST_DLL")"; cp -f "$BASE_DLL" "$DEST_DLL"; fi
printf 'RESTORED_SOURCE_FILES=7\nCREATED_FILES_PRESENT=0\nROLLBACK_TASK17_3=PASS\n'
