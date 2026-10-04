import math
import re
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
SRC = ROOT / "decompiled-src" / "CompleteCheatMenu"
MATH = SRC / "Targeting" / "AimOverlayMath.cs"
ESP = SRC / "Runtime" / "EspRendererV2.cs"
STATE = SRC / "Runtime" / "CheatState.cs"
PRESETS = SRC / "Runtime" / "Presets.cs"
VISUALS = SRC / "UI" / "Tabs" / "VisualsTab.cs"


def angle_to_pixels(vertical_fov, screen_width, screen_height, aim_angle):
    if screen_width <= 0 or screen_height <= 0 or vertical_fov <= 0 or aim_angle <= 0:
        return 0.0
    diagonal = math.hypot(screen_width * 0.5, screen_height * 0.5)
    if aim_angle >= 89.9:
        return diagonal
    focal = (screen_height * 0.5) / math.tan(math.radians(vertical_fov * 0.5))
    return min(diagonal, focal * math.tan(math.radians(aim_angle)))


assert abs(angle_to_pixels(60.0, 1920, 1080, 30.0) - 540.0) < 0.001
assert abs(angle_to_pixels(60.0, 1920, 1080, 10.0) - 164.925) < 0.01
assert abs(angle_to_pixels(60.0, 1920, 1080, 90.0) - 1101.454) < 0.01
assert angle_to_pixels(0.0, 1920, 1080, 30.0) == 0.0

math_source = MATH.read_text(encoding="utf-8")
assert "AngleToPixelRadius" in math_source
assert "EffectiveRadius" in math_source
assert "Mathf.Tan" in math_source
assert "camera.fieldOfView" not in math_source
assert "Mathf.Min(screenRadius, angleRadius)" in math_source

esp = ESP.read_text(encoding="utf-8")
assert "AimOverlayMath.AngleToPixelRadius(camera.fieldOfView" in esp
assert "AimOverlayMath.EffectiveRadius(CheatState.AimScreenRadius" in esp
assert "DrawCurrentTargetTracer(camera" in esp
assert re.search(r"DrawAimOverlay\(camera\);.*?if \(!CheatState\.EspEnabled\)", esp, re.S)
assert "CheatState.EspAimTargetTracer" in esp
assert "WeaponCheats.CurrentTarget" in esp
assert "target.AimPoint" in esp
assert "new Vector2(Screen.width * 0.5f, Screen.height * 0.5f)" in esp

state = STATE.read_text(encoding="utf-8")
presets = PRESETS.read_text(encoding="utf-8")
visuals = VISUALS.read_text(encoding="utf-8")
assert "internal static bool EspAimTargetTracer = true;" in state
assert 'B("espAimTargetTracer", CheatState.EspAimTargetTracer)' in presets
assert 'case "espAimTargetTracer"' in presets
assert "当前目标连线" in visuals

print("AIM_OVERLAY_TEST=PASS cases=22")
