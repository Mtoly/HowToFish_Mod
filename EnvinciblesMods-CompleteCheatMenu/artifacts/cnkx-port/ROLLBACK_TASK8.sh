#!/usr/bin/env bash
set -euo pipefail
ROOT="${1:-/mnt/d/Code/How2fish/EnvinciblesMods-CompleteCheatMenu}"
SOURCE_ROOT="/mnt/d/Code/How2fish/EnvinciblesMods-CompleteCheatMenu/artifacts/cnkx-port/task8-original-src"
[[ -d "$ROOT" ]] || { printf 'DestinationRoot does not exist: %s\n' "$ROOT" >&2; exit 2; }
mkdir -p "$ROOT/decompiled-src/CompleteCheatMenu/Runtime" "$ROOT/decompiled-src/CompleteCheatMenu/Targeting" "$ROOT/tests"
cp -- "$SOURCE_ROOT/CheatState.cs" "$ROOT/decompiled-src/CompleteCheatMenu/Runtime/CheatState.cs"
cp -- "$SOURCE_ROOT/TargetingSystem.cs" "$ROOT/decompiled-src/CompleteCheatMenu/Targeting/TargetingSystem.cs"
cp -- "$SOURCE_ROOT/cnkx_math_impl.py" "$ROOT/tests/cnkx_math_impl.py"
cp -- "$SOURCE_ROOT/ballistic_prediction_test.py" "$ROOT/tests/ballistic_prediction_test.py"
created=(
  "decompiled-src/CompleteCheatMenu/Targeting/VelocityTracker.cs"
  "decompiled-src/CompleteCheatMenu/Targeting/BallisticPredictor.cs"
)
for relative in "${created[@]}"; do
  [[ -e "$ROOT/$relative" ]] && rm -- "$ROOT/$relative"
done
cheat="$(sha256sum "$ROOT/decompiled-src/CompleteCheatMenu/Runtime/CheatState.cs" | awk '{print $1}')"
system="$(sha256sum "$ROOT/decompiled-src/CompleteCheatMenu/Targeting/TargetingSystem.cs" | awk '{print $1}')"
mathhash="$(sha256sum "$ROOT/tests/cnkx_math_impl.py" | awk '{print $1}')"
testhash="$(sha256sum "$ROOT/tests/ballistic_prediction_test.py" | awk '{print $1}')"
remaining=0
for relative in "${created[@]}"; do
  [[ -e "$ROOT/$relative" ]] && remaining=$((remaining + 1))
done
printf 'CHEATSTATE_SHA256=%s\n' "${cheat^^}"
printf 'TARGETING_SYSTEM_SHA256=%s\n' "${system^^}"
printf 'MATH_IMPL_SHA256=%s\n' "${mathhash^^}"
printf 'BALLISTIC_TEST_SHA256=%s\n' "${testhash^^}"
printf 'CREATED_REMAINING=%d\n' "$remaining"
if [[ "$cheat" == "9761a9e2e295b6d44bfcdf87c4d9ed6ebd1fddf11d4c3ab142b9b1e2481840e6" && "$system" == "139c1e8a5f58b8be4c5432bbc1359a07395059e9dcf0b30d62309dc41dddc747" && "$mathhash" == "5c2f5f73f5861a5c2aea2bad50c97c9ea101647be8d3d0679e2ca53920d8e929" && "$testhash" == "1c1819893c1f93891f66ca94a878367b97048d3adf0e6345e2167c8ad928f258" && "$remaining" -eq 0 ]]; then
  printf 'ROLLBACK_MATCH=True\n'
else
  printf 'ROLLBACK_MATCH=False\n'
  exit 2
fi
