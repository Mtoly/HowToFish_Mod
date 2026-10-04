#!/usr/bin/env bash
set -euo pipefail
ROOT="${1:-/mnt/d/Code/How2fish/EnvinciblesMods-CompleteCheatMenu}"
SOURCE_ROOT="/mnt/d/Code/How2fish/EnvinciblesMods-CompleteCheatMenu/artifacts/cnkx-port/task9-original-src"
[[ -d "$ROOT" ]] || { printf 'DestinationRoot does not exist: %s\n' "$ROOT" >&2; exit 2; }
mkdir -p "$ROOT/decompiled-src/CompleteCheatMenu/Patches" "$ROOT/decompiled-src/CompleteCheatMenu/Cheats" "$ROOT/decompiled-src/CompleteCheatMenu/Runtime" "$ROOT/tests"
cp -- "$SOURCE_ROOT/WeaponShoot_Patch.cs" "$ROOT/decompiled-src/CompleteCheatMenu/Patches/WeaponShoot_Patch.cs"
cp -- "$SOURCE_ROOT/WeaponCheats.cs" "$ROOT/decompiled-src/CompleteCheatMenu/Cheats/WeaponCheats.cs"
cp -- "$SOURCE_ROOT/CheatState.cs" "$ROOT/decompiled-src/CompleteCheatMenu/Runtime/CheatState.cs"
cp -- "$SOURCE_ROOT/cnkx_math_impl.py" "$ROOT/tests/cnkx_math_impl.py"
cp -- "$SOURCE_ROOT/tracking_probability_test.py" "$ROOT/tests/tracking_probability_test.py"
created="$ROOT/decompiled-src/CompleteCheatMenu/Targeting/ShotRedirector.cs"
[[ -e "$created" ]] && rm -- "$created"
patch_hash="$(sha256sum "$ROOT/decompiled-src/CompleteCheatMenu/Patches/WeaponShoot_Patch.cs" | awk '{print $1}')"
weapon="$(sha256sum "$ROOT/decompiled-src/CompleteCheatMenu/Cheats/WeaponCheats.cs" | awk '{print $1}')"
cheat="$(sha256sum "$ROOT/decompiled-src/CompleteCheatMenu/Runtime/CheatState.cs" | awk '{print $1}')"
mathhash="$(sha256sum "$ROOT/tests/cnkx_math_impl.py" | awk '{print $1}')"
testhash="$(sha256sum "$ROOT/tests/tracking_probability_test.py" | awk '{print $1}')"
remaining=0
[[ -e "$created" ]] && remaining=1
printf 'SHOOT_PATCH_SHA256=%s\n' "${patch_hash^^}"
printf 'WEAPON_SHA256=%s\n' "${weapon^^}"
printf 'CHEATSTATE_SHA256=%s\n' "${cheat^^}"
printf 'MATH_IMPL_SHA256=%s\n' "${mathhash^^}"
printf 'TRACKING_TEST_SHA256=%s\n' "${testhash^^}"
printf 'CREATED_REMAINING=%d\n' "$remaining"
if [[ "$patch_hash" == "b83ed4f0e6c0ee4f35e03f7b8259a87f9e9ac01b3b081d6f7e0364527f0f16b3" && "$weapon" == "b99444c00d46140ce35a73251610000f3c5d718489f23149aa894b8c705310c6" && "$cheat" == "3de73e553c2dc03615e73453a2c088f8f5484e560e6aadc5e585642224f5c386" && "$mathhash" == "dd15d856193d6c8f18d3bf221ed7fbfd7921fed7bbda9afdf841a53f19728eed" && "$testhash" == "5ca9a34e3248527ddb739d6e2b4a0eaf6847fa9153667a795b058d6940352608" && "$remaining" -eq 0 ]]; then
  printf 'ROLLBACK_MATCH=True\n'
else
  printf 'ROLLBACK_MATCH=False\n'
  exit 2
fi
