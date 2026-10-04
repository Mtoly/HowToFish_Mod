#!/usr/bin/env bash
set -euo pipefail

HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="${1:-/mnt/d/Code/How2fish/EnvinciblesMods-CompleteCheatMenu}"
GAME_DLL="${2:-/mnt/e/SteamLibrary/steamapps/common/How to Fish/How to Fish/BepInEx/plugins/EnvinciblesMods-CompleteCheatMenu/CompleteCheatMenu.dll}"

declare -A restore=(
	['tools/localize_dll.py']='tools__localize_dll.py'
	['decompiled-src/CompleteCheatMenu/UI/Tabs/GamblingTab.cs']='decompiled-src__CompleteCheatMenu__UI__Tabs__GamblingTab.cs'
	['docs/plans/2026-08-25-complete-cheat-menu-cnkx-port-implementation-plan.md']='docs__plans__2026-08-25-complete-cheat-menu-cnkx-port-implementation-plan.md'
)
for relative in "${!restore[@]}"; do
	mkdir -p "$(dirname "$PROJECT_ROOT/$relative")"
	cp -f "$HERE/original/${restore[$relative]}" "$PROJECT_ROOT/$relative"
done
rm -f "$PROJECT_ROOT/tests/gambling_colour_localization_test.py"
mkdir -p "$(dirname "$GAME_DLL")"
cp -f "$HERE/original/CompleteCheatMenu.deployed.dll" "$GAME_DLL"
expected='a105a0e1ab2a67a252387833000bafffde46e3b6fc3aa8a7a6a708cf7404d0e0'
actual="$(sha256sum "$GAME_DLL" | awk '{print $1}')"
[[ "$actual" == "$expected" ]]
present=0
[[ -e "$PROJECT_ROOT/tests/gambling_colour_localization_test.py" ]] && present=1
printf 'ROLLBACK_GAMBLING_FIX=PASS\nRESTORED_DLL_SHA256=%s\nRESTORED_SOURCE_FILES=%s\nCREATED_FILES_PRESENT=%s\n' "${actual^^}" "${#restore[@]}" "$present"
