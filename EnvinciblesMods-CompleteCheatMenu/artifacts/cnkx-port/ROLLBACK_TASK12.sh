#!/usr/bin/env bash
set -euo pipefail
ROOT="${1:-/mnt/d/Code/How2fish/EnvinciblesMods-CompleteCheatMenu}"
SOURCE_ROOT="/mnt/d/Code/How2fish/EnvinciblesMods-CompleteCheatMenu/artifacts/cnkx-port/task12-original-src"
[[ -d "$ROOT" ]] || { printf 'DestinationRoot does not exist: %s\n' "$ROOT" >&2; exit 2; }
mkdir -p "$ROOT/decompiled-src/CompleteCheatMenu/Runtime" "$ROOT/tests"
cp -- "$SOURCE_ROOT/EspRenderer.cs" "$ROOT/decompiled-src/CompleteCheatMenu/Runtime/EspRenderer.cs"
cp -- "$SOURCE_ROOT/CheatState.cs" "$ROOT/decompiled-src/CompleteCheatMenu/Runtime/CheatState.cs"
cp -- "$SOURCE_ROOT/Plugin.cs" "$ROOT/decompiled-src/CompleteCheatMenu/Plugin.cs"
created=(
  "decompiled-src/CompleteCheatMenu/Runtime/EspRendererV2.cs"
  "decompiled-src/CompleteCheatMenu/Runtime/GuiPrimitives.cs"
  "tests/cnkx_esp_projection_impl.py"
  "tests/esp_projection_test.py"
)
for relative in "${created[@]}"; do
  [[ -e "$ROOT/$relative" ]] && rm -- "$ROOT/$relative"
done
esp="$(sha256sum "$ROOT/decompiled-src/CompleteCheatMenu/Runtime/EspRenderer.cs" | awk '{print $1}')"
cheat="$(sha256sum "$ROOT/decompiled-src/CompleteCheatMenu/Runtime/CheatState.cs" | awk '{print $1}')"
plugin="$(sha256sum "$ROOT/decompiled-src/CompleteCheatMenu/Plugin.cs" | awk '{print $1}')"
remaining=0
for relative in "${created[@]}"; do
  [[ -e "$ROOT/$relative" ]] && remaining=$((remaining + 1))
done
printf 'ESP_RENDERER_SHA256=%s\n' "${esp^^}"
printf 'CHEATSTATE_SHA256=%s\n' "${cheat^^}"
printf 'PLUGIN_SHA256=%s\n' "${plugin^^}"
printf 'CREATED_REMAINING=%d\n' "$remaining"
if [[ "$esp" == "122a3aebb550bc294c18710dc6fd2e9cd3975cb4c7dddc52f99fa12fd296d87d" && "$cheat" == "e028f72e3b5eddc98786dbd89d322cd7b2daa5f44c763ecc4d092284af78640d" && "$plugin" == "daf9983f40dcafcd2efb936d346eabdebffcac088e5f208e459d79594fa45bec" && "$remaining" -eq 0 ]]; then
  printf 'ROLLBACK_MATCH=True\n'
else
  printf 'ROLLBACK_MATCH=False\n'
  exit 2
fi
