from pathlib import Path
import difflib


ROOT = Path(r"D:\Code\How2fish\EnvinciblesMods-CompleteCheatMenu")
OUT = ROOT / "artifacts" / "gambling-force-colour-fix"
ORIGINAL = OUT / "original"
CHANGED = {
    "tools/localize_dll.py": "tools__localize_dll.py",
    "decompiled-src/CompleteCheatMenu/UI/Tabs/GamblingTab.cs": "decompiled-src__CompleteCheatMenu__UI__Tabs__GamblingTab.cs",
    "docs/plans/2026-08-25-complete-cheat-menu-cnkx-port-implementation-plan.md": "docs__plans__2026-08-25-complete-cheat-menu-cnkx-port-implementation-plan.md",
}
CREATED = ["tests/gambling_colour_localization_test.py"]

parts = []
for relative, baseline in CHANGED.items():
    before = (ORIGINAL / baseline).read_text(encoding="utf-8").splitlines(keepends=True)
    after = (ROOT / relative).read_text(encoding="utf-8").splitlines(keepends=True)
    parts.extend(difflib.unified_diff(before, after, fromfile="a/" + relative, tofile="b/" + relative))
for relative in CREATED:
    after = (ROOT / relative).read_text(encoding="utf-8").splitlines(keepends=True)
    parts.extend(difflib.unified_diff([], after, fromfile="/dev/null", tofile="b/" + relative))

target = OUT / "gambling-force-colour-fix.diff"
target.write_text("".join(parts), encoding="utf-8")
print(f"DIFF_FILE={target}")
print(f"DIFF_LINES={len(parts)}")
