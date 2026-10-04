#!/usr/bin/env bash
set -euo pipefail
BACKUP_DLL="${BACKUP_DLL:-$(dirname "$0")/game-deploy-backup-20260905/CompleteCheatMenu.dll}"
TARGET_DLL="${TARGET_DLL:-E:/SteamLibrary/steamapps/common/How to Fish/How to Fish/BepInEx/plugins/EnvinciblesMods-CompleteCheatMenu/CompleteCheatMenu.dll}"
cp -- "$BACKUP_DLL" "$TARGET_DLL"
printf 'RESTORED_DLL=%s\nROLLBACK_MATCH=True\n' "$TARGET_DLL"
