#!/usr/bin/env bash
set -euo pipefail
ROOT="${1:-/mnt/d/Code/How2fish/EnvinciblesMods-CompleteCheatMenu}"
SOURCE_ROOT="/mnt/d/Code/How2fish/EnvinciblesMods-CompleteCheatMenu/artifacts/cnkx-port/task14-original-src"
[[ -d "$ROOT" ]] || { printf 'DestinationRoot does not exist: %s\n' "$ROOT" >&2; exit 2; }
plan='docs/plans/2026-08-25-complete-cheat-menu-cnkx-port-implementation-plan.md'
mkdir -p "$(dirname "$ROOT/$plan")" "$ROOT/artifacts"
cp -- "$SOURCE_ROOT/2026-08-25-complete-cheat-menu-cnkx-port-implementation-plan.md" "$ROOT/$plan"
cp -- "$SOURCE_ROOT/CompleteCheatMenu.baseline.dll" "$ROOT/artifacts/CompleteCheatMenu.dll"
created=(
  'tests/cnkx_performance_harness.cs'
  'artifacts/cnkx-port/performance.txt'
  'artifacts/cnkx-port/review.md'
  'artifacts/cnkx-port/cnkx_performance_harness.exe'
)
for relative in "${created[@]}"; do rm -f -- "$ROOT/$relative"; done
dll_hash="$(sha256sum "$ROOT/artifacts/CompleteCheatMenu.dll" | awk '{print toupper($1)}')"
remaining=0
for relative in "${created[@]}"; do [[ -e "$ROOT/$relative" ]] && remaining=$((remaining+1)); done
printf 'PLAN_SHA256=%s\n' "$(sha256sum "$ROOT/$plan" | awk '{print toupper($1)}')"
printf 'DLL_SHA256=%s\n' "$dll_hash"
printf 'CREATED_REMAINING=%d\n' "$remaining"
if [[ "$dll_hash" == 'AB57B01D8326EC3472CA8AC306EB1D434A4B046A819EFC07D456DC36A2498391' && "$remaining" -eq 0 ]]; then
  printf 'ROLLBACK_MATCH=True\n'
else
  printf 'ROLLBACK_MATCH=False\n'
  exit 2
fi
exit 0
