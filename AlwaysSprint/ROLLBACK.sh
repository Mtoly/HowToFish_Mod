#!/usr/bin/env bash
set -euo pipefail
target="${1:?path to AlwaysSprint.dll}"
case "$(basename "$target")" in AlwaysSprint.dll) ;; *) echo "Target must be AlwaysSprint.dll" >&2; exit 2;; esac
rm -f -- "$target"
rmdir --ignore-fail-on-non-empty -- "$(dirname "$target")" 2>/dev/null || true
echo 'rollback=removed-new-plugin'
