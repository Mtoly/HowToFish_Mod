#!/usr/bin/env bash
set -euo pipefail
ROOT="${1:-/mnt/d/Code/How2fish/EnvinciblesMods-CompleteCheatMenu}"
SOURCE_ROOT="/mnt/d/Code/How2fish/EnvinciblesMods-CompleteCheatMenu/artifacts/cnkx-port/task5-original-src"
[[ -d "$ROOT" ]] || { printf 'DestinationRoot does not exist: %s\n' "$ROOT" >&2; exit 2; }
mkdir -p "$ROOT/decompiled-src/CompleteCheatMenu/Runtime" "$ROOT/tests"
cp -- "$SOURCE_ROOT/CheatState.cs" "$ROOT/decompiled-src/CompleteCheatMenu/Runtime/CheatState.cs"
cp -- "$SOURCE_ROOT/targeting_math_test.py" "$ROOT/tests/targeting_math_test.py"
created=(
  "decompiled-src/CompleteCheatMenu/Targeting/TargetCandidate.cs"
  "decompiled-src/CompleteCheatMenu/Targeting/TargetSolution.cs"
  "decompiled-src/CompleteCheatMenu/Targeting/TargetingSettings.cs"
  "decompiled-src/CompleteCheatMenu/Targeting/TargetScorer.cs"
  "tests/cnkx_math_impl.py"
)
for relative in "${created[@]}"; do
  [[ -e "$ROOT/$relative" ]] && rm -- "$ROOT/$relative"
done
cheat="$(sha256sum "$ROOT/decompiled-src/CompleteCheatMenu/Runtime/CheatState.cs" | awk '{print $1}')"
testhash="$(sha256sum "$ROOT/tests/targeting_math_test.py" | awk '{print $1}')"
remaining=0
for relative in "${created[@]}"; do
  [[ -e "$ROOT/$relative" ]] && remaining=$((remaining + 1))
done
printf 'CHEATSTATE_SHA256=%s\n' "${cheat^^}"
printf 'TARGETING_TEST_SHA256=%s\n' "${testhash^^}"
printf 'CREATED_REMAINING=%d\n' "$remaining"
if [[ "$cheat" == "b3fccaaf0c3b64cd036667f3e762c6831c14d710d652b8e883a410047aa6a553" && "$testhash" == "96b18c0f3d0810b44e809f633e0682f616c5fbd6615bff40ccbdcb76aa7d9c0f" && "$remaining" -eq 0 ]]; then
  printf 'ROLLBACK_MATCH=True\n'
else
  printf 'ROLLBACK_MATCH=False\n'
  exit 2
fi
