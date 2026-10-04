#!/usr/bin/env bash
set -euo pipefail
destination="${1:-/mnt/e/SteamLibrary/steamapps/common/How to Fish/How to Fish/BepInEx/plugins/EnvinciblesMods-CompleteCheatMenu/CompleteCheatMenu.dll}"
baseline='/mnt/d/Code/How2fish/EnvinciblesMods-CompleteCheatMenu/artifacts/cnkx-port/CompleteCheatMenu.pre-cnkx.dll'
expected='F4B5209647A60C0BE7E5C49005867A4CE094FEB56C03AD804AEC67C35FE3BA79'
mkdir -p "$(dirname "$destination")"
cp -- "$baseline" "$destination"
actual="$(sha256sum "$destination" | awk '{print toupper($1)}')"
printf 'RESTORED_DLL=%s\n' "$destination"
printf 'RESTORED_SHA256=%s\n' "$actual"
if [[ "$actual" == "$expected" ]]; then
  printf 'ROLLBACK_MATCH=True\n'
else
  printf 'ROLLBACK_MATCH=False\n'
  exit 2
fi
exit 0
