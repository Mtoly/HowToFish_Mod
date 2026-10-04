import math
import sys
from pathlib import Path

TESTS = Path(__file__).resolve().parent
PROJECT = TESTS.parent
sys.path.insert(0, str(TESTS))

try:
    from cnkx_esp_projection_impl import fallback_rect, project_bounds
except (ImportError, ModuleNotFoundError):
    print("ESP_PROJECTION_TESTS=FAIL missing implementation")
    raise SystemExit(1)


# 1. 八角点包围盒投影得到真实的屏幕边界，而不是固定尺寸标记。
box = project_bounds(
    center=(0.0, 0.0, 10.0),
    extents=(1.0, 2.0, 1.0),
    screen=(1920.0, 1080.0),
    vertical_fov=90.0,
)
assert box is not None
assert box[0] < 960.0 < box[2]
assert box[1] < 540.0 < box[3]
assert box[2] - box[0] > 100.0
assert box[3] - box[1] > 200.0

# 2. 增大世界包围盒时，屏幕框同步变大。
larger = project_bounds(
    center=(0.0, 0.0, 10.0),
    extents=(2.0, 3.0, 1.0),
    screen=(1920.0, 1080.0),
    vertical_fov=90.0,
)
assert larger[2] - larger[0] > box[2] - box[0]
assert larger[3] - larger[1] > box[3] - box[1]

# 3. 完全位于相机后的包围盒不得生成屏幕框。
assert (
    project_bounds(
        center=(0.0, 0.0, -10.0),
        extents=(1.0, 2.0, 1.0),
        screen=(1920.0, 1080.0),
        vertical_fov=90.0,
    )
    is None
)

# 4. 穿过近裁剪面的包围盒拒绝投影，避免产生跨越整个屏幕的异常框。
assert (
    project_bounds(
        center=(0.0, 0.0, 0.5),
        extents=(1.0, 1.0, 1.0),
        screen=(1920.0, 1080.0),
        vertical_fov=90.0,
    )
    is None
)

# 5. 无 Renderer 时的回退框稳定、有限且保持固定宽高比。
fallback = fallback_rect(
    root=(0.0, 0.0, 12.0),
    height=1.8,
    screen=(1920.0, 1080.0),
    vertical_fov=90.0,
)
assert fallback is not None
width = fallback[2] - fallback[0]
height = fallback[3] - fallback[1]
assert math.isfinite(width) and math.isfinite(height)
assert width > 1.0 and height > 1.0
assert math.isclose(width / height, 0.5, rel_tol=1e-6)

# 根节点横倒或翻转时仍使用世界竖直方向，回退框保持一致。
rotated_fallback = fallback_rect(
    root=(0.0, 0.0, 12.0),
    height=1.8,
    screen=(1920.0, 1080.0),
    vertical_fov=90.0,
    root_up=(1.0, 0.0, 0.0),
)
assert rotated_fallback == fallback

# 6. 屏幕外但仍在前方的实体保持有限边界，交由生产实现做可见区域裁剪。
offscreen = project_bounds(
    center=(25.0, 0.0, 10.0),
    extents=(1.0, 2.0, 1.0),
    screen=(1920.0, 1080.0),
    vertical_fov=90.0,
)
assert offscreen is not None
assert all(math.isfinite(value) for value in offscreen)
assert offscreen[0] > 1920.0

# 7. 生产实现必须具备 Repaint 门控、八角点投影、稳定回退框和目标高亮。
renderer = (
    PROJECT / "decompiled-src/CompleteCheatMenu/Runtime/EspRendererV2.cs"
).read_text(encoding="utf-8")
assert "EventType.Repaint" in renderer
assert "TryProjectBounds" in renderer
assert "GetCorners" in renderer
assert "TryProjectFallback" in renderer
assert "WeaponCheats.CurrentTarget" in renderer
assert "DrawCircle" in renderer
assert "DrawSkeleton" in renderer
assert "TryReadHealth" in renderer
assert "BuildDetails" in renderer
assert "GeometryCache" in renderer
assert "EspVisibilityCache" in renderer
assert "animator.isHuman" in renderer
assert "root.position + Vector3.up * 1.8f" in renderer
# ESP bounds-center visibility stays independent from the targeting aim-point
# result, so disabling line-of-sight targeting cannot mark wall targets visible.
assert "return current.Visible" not in renderer
assert "Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore" in renderer

# 8. 所有基础绘制函数必须在 finally 中恢复 GUI.color 和 GUI.matrix。
primitives = (
    PROJECT / "decompiled-src/CompleteCheatMenu/Runtime/GuiPrimitives.cs"
).read_text(encoding="utf-8")
assert "Color previousColor = GUI.color" in primitives
assert "Matrix4x4 previousMatrix = GUI.matrix" in primitives
assert primitives.count("finally") >= 4
assert primitives.count("GUI.color = previousColor") >= 4
assert primitives.count("GUI.matrix = previousMatrix") >= 4

plugin = (PROJECT / "decompiled-src/CompleteCheatMenu/Plugin.cs").read_text(encoding="utf-8")
wrapper = (
    PROJECT / "decompiled-src/CompleteCheatMenu/Runtime/EspRenderer.cs"
).read_text(encoding="utf-8")
assert "SafeRun(EspRenderer.Draw" in plugin
assert "EspRendererV2.Draw();" in wrapper

print("ESP_PROJECTION_TESTS=PASS cases=8")
