#!/usr/bin/env bash
set -euo pipefail
SRC="${1:-CompleteCheatMenu.dll}"
DST="${2:-CompleteCheatMenu.zh-CN.dll}"
cp -- "$SRC" "$DST"
sha256sum -- "$DST"
