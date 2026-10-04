#!/usr/bin/env bash
set -euo pipefail
ROOT="${1:-/mnt/d/Code/How2fish/EnvinciblesMods-CompleteCheatMenu}"
FILES=(
  "tests/targeting_math_test.py"
  "tests/ballistic_prediction_test.py"
  "tests/tracking_probability_test.py"
)
removed=0
for relative in "${FILES[@]}"; do
  target="$ROOT/$relative"
  if [[ -f "$target" ]]; then
    rm -- "$target"
    removed=$((removed + 1))
  fi
done
remaining=0
for relative in "${FILES[@]}"; do
  [[ -e "$ROOT/$relative" ]] && remaining=$((remaining + 1))
done
printf 'ROLLBACK_REMOVED=%d\n' "$removed"
printf 'ROLLBACK_REMAINING=%d\n' "$remaining"
if [[ "$remaining" -eq 0 ]]; then
  printf 'ROLLBACK_MATCH=True\n'
else
  printf 'ROLLBACK_MATCH=False\n'
  exit 2
fi
