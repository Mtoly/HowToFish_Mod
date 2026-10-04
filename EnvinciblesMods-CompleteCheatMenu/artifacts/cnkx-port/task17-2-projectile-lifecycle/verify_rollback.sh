#!/usr/bin/env bash
set -euo pipefail
WS=/mnt/d/Code/How2fish/EnvinciblesMods-CompleteCheatMenu
ART="$WS/artifacts/cnkx-port/task17-2-projectile-lifecycle"
TEST="$ART/rollback-test-sh-20260825"
case "$TEST" in
  "$ART"/*) ;;
  *) exit 90 ;;
esac
rm -rf -- "$TEST"
mkdir -p "$TEST"
cp -a "$WS/decompiled-src" "$WS/docs" "$WS/tests" "$TEST/"
cp "$ART/CompleteCheatMenu.task17-2.zh-CN.dll" "$TEST/deployed.dll"
bash "$ART/ROLLBACK_TASK17_2.sh" "$TEST" "$TEST/deployed.dll" \
  "$WS/artifacts/cnkx-port/task17-1-visibility-ui/CompleteCheatMenu.task17-1.zh-CN.dll"
hash="$(sha256sum "$TEST/deployed.dll")"
hash="${hash%% *}"
test "$hash" = ed9f679fd8ae6dcf12a0171a01252dbb2cf0a3cf5c63dd2e02a4fad6e9192b6b
test ! -e "$TEST/decompiled-src/CompleteCheatMenu/Targeting/ProjectileBindings.cs"
test ! -e "$TEST/decompiled-src/CompleteCheatMenu/Targeting/ProjectileOwnership.cs"
test ! -e "$TEST/decompiled-src/CompleteCheatMenu/Targeting/ProjectileTracker.cs"
test ! -e "$TEST/decompiled-src/CompleteCheatMenu/Targeting/MagicShotContext.cs"
test ! -e "$TEST/tests/projectile_lifecycle_test.py"
test ! -e "$TEST/tests/projectile_lifecycle_harness.cs"
printf 'ROLLBACK_SH_DLL_SHA256=%s\n' "$hash"
printf 'ROLLBACK_SH_CREATED_FILES_PRESENT=0\n'
printf 'ROLLBACK_SH_VERIFICATION=PASS\n'
