#!/usr/bin/env bash
set -euo pipefail

game_dll="${1:-/mnt/e/SteamLibrary/steamapps/common/How to Fish/How to Fish/BepInEx/plugins/Kai935-FishAimbot/AimbotRevised.dll}"
backup='/mnt/d/Code/How2fish/Kai935-FishAimbot/artifacts/AimbotRevised.original.dll'
expected='F779756D916863675EF2DA7372616B8D64DB1736505409B8CC0D41A5A575B47D'
cp -f -- "$backup" "$game_dll"
actual="$(sha256sum "$game_dll" | awk '{print toupper($1)}')"
printf 'ROLLBACK_SHA256=%s\n' "$actual"
test "$actual" = "$expected"
