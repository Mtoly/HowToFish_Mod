from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
SRC = ROOT / "decompiled-src" / "CompleteCheatMenu"

weapons = (SRC / "UI" / "Tabs" / "WeaponsTab.cs").read_text(encoding="utf-8")
targeting = (SRC / "Targeting" / "TargetingSystem.cs").read_text(encoding="utf-8")
visibility = (SRC / "Targeting" / "VisibilityCache.cs").read_text(encoding="utf-8")
esp = (SRC / "Runtime" / "EspRendererV2.cs").read_text(encoding="utf-8")
state = (SRC / "Runtime" / "CheatState.cs").read_text(encoding="utf-8")
presets = (SRC / "Runtime" / "Presets.cs").read_text(encoding="utf-8")

# Seam 1: user-facing labels describe world distance and screen-space aim radius.
assert 'SliderRow("最大距离", CheatState.AimRange' in weapons
assert 'SliderRow("自瞄范围", CheatState.AimScreenRadius' in weapons
assert 'SliderRow("最大范围", CheatState.AimRange' not in weapons
assert 'SliderRow("屏幕锁定半径", CheatState.AimScreenRadius' not in weapons
assert "自瞄范围使用屏幕像素半径" in weapons

# Seam 2: actual visibility is measured even when occluded targets are eligible.
assert "bool visible = _visibility.IsVisible(" in targeting
assert "bool visible = !settings.RequireLineOfSight ||" not in targeting
assert "if (settings.RequireLineOfSight && !visible)" in targeting

# Seam 3: targeting and ESP use the same layer/trigger policy while retaining
# independent aim-point and bounds-center caches.
raycast_policy = "Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore"
assert raycast_policy in visibility
assert raycast_policy in esp
assert "return current.Visible;" not in esp
assert "EspVisibilityCache" in esp

# Seam 4: ESP distance remains a single shared setting and preset key.
assert "internal static float EspDistance = 150f;" in state
for forbidden in (
    "EspPlayerDistance",
    "EspCreatureDistance",
    "EspItemDistance",
    "EspContainerDistance",
):
    assert forbidden not in state
    assert forbidden not in presets
assert presets.count('"espDistance"') >= 2

print("VISIBILITY_UI_SEMANTICS_TESTS=PASS cases=4")
