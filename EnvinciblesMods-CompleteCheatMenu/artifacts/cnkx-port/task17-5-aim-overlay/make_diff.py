from pathlib import Path
import difflib
root=Path(r"D:\Code\How2fish\EnvinciblesMods-CompleteCheatMenu")
out=root/'artifacts/cnkx-port/task17-5-aim-overlay'
orig=out/'original/source'
changed=[
 'decompiled-src/CompleteCheatMenu/Runtime/EspRendererV2.cs',
 'decompiled-src/CompleteCheatMenu/Runtime/CheatState.cs',
 'decompiled-src/CompleteCheatMenu/Runtime/Presets.cs',
 'decompiled-src/CompleteCheatMenu/UI/Tabs/VisualsTab.cs',
 'tests/localized_ui_preset_test.py',
 'docs/plans/2026-08-25-complete-cheat-menu-cnkx-port-implementation-plan.md',
]
created=[
 'decompiled-src/CompleteCheatMenu/Targeting/AimOverlayMath.cs',
 'tests/aim_overlay_test.py',
 'tests/aim_overlay_harness.cs',
]
parts=[]
for rel in changed:
 a=(orig/rel).read_text(encoding='utf-8').splitlines(keepends=True)
 b=(root/rel).read_text(encoding='utf-8').splitlines(keepends=True)
 parts.extend(difflib.unified_diff(a,b,fromfile='a/'+rel,tofile='b/'+rel))
for rel in created:
 b=(root/rel).read_text(encoding='utf-8').splitlines(keepends=True)
 parts.extend(difflib.unified_diff([],b,fromfile='/dev/null',tofile='b/'+rel))
path=out/'task17-5-aim-overlay.diff'
path.write_text(''.join(parts),encoding='utf-8')
print(f'DIFF_FILE={path}')
print(f'DIFF_LINES={len(parts)}')
