#!/usr/bin/env bash
set -euo pipefail
WS=/mnt/d/Code/How2fish/EnvinciblesMods-CompleteCheatMenu;ART="$WS/artifacts/cnkx-port/task17-4-projectile-guidance";TEST="$ART/rollback-test-sh-20260825"
case "$TEST" in "$ART"/*);;*)exit 90;;esac
rm -rf -- "$TEST";mkdir -p "$TEST";cp -a "$WS/decompiled-src" "$WS/docs" "$WS/tests" "$TEST/";cp "$ART/CompleteCheatMenu.task17-4.zh-CN.dll" "$TEST/deployed.dll"
bash "$ART/ROLLBACK_TASK17_4.sh" "$TEST" "$TEST/deployed.dll" "$WS/artifacts/cnkx-port/task17-3-initial-velocity/CompleteCheatMenu.task17-3.zh-CN.dll"
hash="$(sha256sum "$TEST/deployed.dll")";hash="${hash%% *}";test "$hash" = 71c880055d706399932994c8cc1f9d46f5f5e9edddc5e7bfb8c030a73ea8e72c
test ! -e "$TEST/decompiled-src/CompleteCheatMenu/Targeting/ProjectileGuidance.cs";test ! -e "$TEST/decompiled-src/CompleteCheatMenu/Targeting/ProjectileGuidanceMath.cs";test ! -e "$TEST/decompiled-src/CompleteCheatMenu/Patches/ProjectileUpdateScan_Patch.cs";test ! -e "$TEST/decompiled-src/CompleteCheatMenu/Patches/ProjectileRemove_Patch.cs";test ! -e "$TEST/tests/projectile_guidance_test.py";test ! -e "$TEST/tests/projectile_guidance_harness.cs";test ! -e "$TEST/tests/projectile_guidance_lifecycle_harness.cs"
printf 'ROLLBACK_SH_DLL_SHA256=%s\nROLLBACK_SH_CREATED_FILES_PRESENT=0\nROLLBACK_SH_VERIFICATION=PASS\n' "$hash"
