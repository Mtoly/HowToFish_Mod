#!/usr/bin/env bash
set -euo pipefail
ROOT="${1:-/mnt/d/Code/How2fish/EnvinciblesMods-CompleteCheatMenu}"
SOURCE_ROOT="/mnt/d/Code/How2fish/EnvinciblesMods-CompleteCheatMenu/artifacts/cnkx-port/task13-original-src"
[[ -d "$ROOT" ]] || { printf 'DestinationRoot does not exist: %s\n' "$ROOT" >&2; exit 2; }
declare -A files=(
  [VisualsTab.cs]='decompiled-src/CompleteCheatMenu/UI/Tabs/VisualsTab.cs'
  [WeaponsTab.cs]='decompiled-src/CompleteCheatMenu/UI/Tabs/WeaponsTab.cs'
  [DiagnosticsTab.cs]='decompiled-src/CompleteCheatMenu/UI/Tabs/DiagnosticsTab.cs'
  [MenuWindow.cs]='decompiled-src/CompleteCheatMenu/UI/MenuWindow.cs'
  [Presets.cs]='decompiled-src/CompleteCheatMenu/Runtime/Presets.cs'
  [CheatState.cs]='decompiled-src/CompleteCheatMenu/Runtime/CheatState.cs'
  [TargetingSystem.cs]='decompiled-src/CompleteCheatMenu/Targeting/TargetingSystem.cs'
  [localize_dll.py]='tools/localize_dll.py'
)
declare -A expected=(
  [VisualsTab.cs]='b90a3bcf72cf6e4d7f06c2bac706510bc53e82b6277133664759301d232dc1b9'
  [WeaponsTab.cs]='19d9977fc059932195ad42bf5ebc1a850312ac946b039b16c34e45ea9d9324d3'
  [DiagnosticsTab.cs]='299f27feff318127ae522704e0cf9203401fb97d6290d996ec43151e55811228'
  [MenuWindow.cs]='e748c37b3cc8fe8c69ff0d35fea595d7fd3b0d9387565a7ae20d60315febdf90'
  [Presets.cs]='46558dfb351dfc7b7d466706238a79b1a2d6bcd353fd0e2ed0bb57d34711688c'
  [CheatState.cs]='1d1c0f4a86f8e5372958e5892c2b09864312a32705802e6d858e2261a6520f19'
  [TargetingSystem.cs]='da694ae460a3d6663d5ab1ce311d0f40c657d157518c46a92603ef058386566d'
  [localize_dll.py]='c0af213b0bda2b4db59fa2be3974c99ba61a6b569e19bb8c5caab0539a32ea8a'
)
for name in "${!files[@]}"; do
  destination="$ROOT/${files[$name]}"
  mkdir -p "$(dirname "$destination")"
  cp -- "$SOURCE_ROOT/$name" "$destination"
done
rm -f -- "$ROOT/tests/localized_ui_preset_test.py"
match=true
for name in "${!files[@]}"; do
  actual="$(sha256sum "$ROOT/${files[$name]}" | awk '{print $1}')"
  printf '%s_SHA256=%s\n' "$name" "${actual^^}"
  [[ "$actual" == "${expected[$name]}" ]] || match=false
done
remaining=0
[[ -e "$ROOT/tests/localized_ui_preset_test.py" ]] && remaining=1
printf 'CREATED_REMAINING=%d\n' "$remaining"
if [[ "$match" == true && "$remaining" -eq 0 ]]; then printf 'ROLLBACK_MATCH=True\n'; else printf 'ROLLBACK_MATCH=False\n'; exit 2; fi
