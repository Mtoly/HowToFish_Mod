#!/usr/bin/env bash
set -euo pipefail
WS=/mnt/d/Code/How2fish/EnvinciblesMods-CompleteCheatMenu
ART="$WS/artifacts/cnkx-port/task17-3-initial-velocity"
TEST="$ART/rollback-test-sh-20260825"
case "$TEST" in
  "$ART"/*) ;;
  *) exit 90 ;;
esac
rm -rf -- "$TEST"
mkdir -p "$TEST"
cp -a "$WS/decompiled-src" "$WS/docs" "$WS/tests" "$TEST/"
cp "$ART/CompleteCheatMenu.task17-3.zh-CN.dll" "$TEST/deployed.dll"
bash "$ART/ROLLBACK_TASK17_3.sh" "$TEST" "$TEST/deployed.dll" \
  "$WS/artifacts/cnkx-port/task17-2-projectile-lifecycle/CompleteCheatMenu.task17-2.zh-CN.dll"
hash="$(sha256sum "$TEST/deployed.dll")"
hash="${hash%% *}"
test "$hash" = 9361b9e43a3c77e9e32fd5421c23e51739bb569a099538da024b5877e7558db2
test ! -e "$TEST/decompiled-src/CompleteCheatMenu/Targeting/InitialVelocityRedirector.cs"
test ! -e "$TEST/decompiled-src/CompleteCheatMenu/Targeting/ProjectileSpawnRegistration.cs"
test ! -e "$TEST/decompiled-src/CompleteCheatMenu/Patches/WeaponAddProjectile_Patch.cs"
test ! -e "$TEST/decompiled-src/CompleteCheatMenu/Patches/WeaponAddProjectiles_Patch.cs"
test ! -e "$TEST/tests/projectile_initial_velocity_test.py"
test ! -e "$TEST/tests/projectile_initial_velocity_harness.cs"
test ! -e "$TEST/tests/projectile_spawn_registration_harness.cs"
printf 'ROLLBACK_SH_DLL_SHA256=%s\n' "$hash"
printf 'ROLLBACK_SH_CREATED_FILES_PRESENT=0\n'
printf 'ROLLBACK_SH_VERIFICATION=PASS\n'
