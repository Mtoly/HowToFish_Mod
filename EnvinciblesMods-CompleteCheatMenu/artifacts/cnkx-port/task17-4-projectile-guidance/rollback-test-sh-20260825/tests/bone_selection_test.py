import math
import sys
from pathlib import Path

TESTS = Path(__file__).resolve().parent
PROJECT = TESTS.parent
sys.path.insert(0, str(TESTS))

try:
    from cnkx_bone_impl import resolve_points, select_closest_projected
except (ImportError, ModuleNotFoundError):
    print("BONE_SELECTION_TESTS=FAIL missing implementation")
    raise SystemExit(1)


def labels(points):
    return [point["label"] for point in points]


# 玩家优先使用 Humanoid Animator 的头、胸、骨盆顺序。
player = resolve_points(
    kind="player",
    humanoid={"head": (0, 1.8, 0), "chest": (0, 1.2, 0), "pelvis": (0, 0.8, 0)},
    named={},
    skinned=[],
    bounds=((0, 1, 0), (1, 2, 1)),
    root=(0, 0, 0),
    fixed_height=0.5,
)
assert labels(player)[:3] == ["head", "chest", "pelvis"]

# 生物先使用已知命名骨骼，再使用 SkinnedMeshRenderer 骨骼。
creature_named = resolve_points(
    kind="creature",
    humanoid={},
    named={"Head_Joint": (1, 2, 3), "Spine_02": (1, 1, 3)},
    skinned=[("skull", (9, 9, 9))],
    bounds=None,
    root=(1, 0, 3),
    fixed_height=0.5,
)
assert labels(creature_named)[:2] == ["head", "chest"]
assert creature_named[0]["position"] == (1, 2, 3)

creature_skinned = resolve_points(
    kind="creature",
    humanoid={},
    named={},
    skinned=[("tail", (0, 0, -1)), ("neck", (0, 1.5, 0)), ("body", (0, 1, 0))],
    bounds=None,
    root=(0, 0, 0),
    fixed_height=0.5,
)
assert labels(creature_skinned)[:2] == ["head", "chest"]

# 无骨骼目标回退到包围盒上部、中心和固定高度点。
fallback = resolve_points(
    kind="creature",
    humanoid={},
    named={},
    skinned=[],
    bounds=((10, 20, 30), (4, 8, 6)),
    root=(10, 16, 30),
    fixed_height=0.5,
)
assert labels(fallback) == ["bounds-upper", "body-center", "fixed-height"]
assert fallback[0]["position"] == (10, 22, 30)
assert fallback[1]["position"] == (10, 20, 30)
assert fallback[2]["position"] == (10, 16.5, 30)

# 指哪打哪只考虑屏幕前方、投影有限的点，并选择离准星最近者。
projected = [
    {"label": "behind", "screen": (100, 100, -1)},
    {"label": "invalid", "screen": (math.nan, 100, 1)},
    {"label": "far", "screen": (130, 140, 1)},
    {"label": "near", "screen": (104, 97, 1)},
]
best = select_closest_projected(projected, center=(100, 100))
assert best["label"] == "near"
assert math.isclose(best["screen_distance"], 5.0)

# 生产源码应直接使用 Animator humanoid 骨骼，并保留 Renderer Bounds 回退。
bone_source = (
    PROJECT / "decompiled-src/CompleteCheatMenu/Targeting/BoneResolver.cs"
).read_text(encoding="utf-8")
bounds_source = (
    PROJECT / "decompiled-src/CompleteCheatMenu/Targeting/RendererBoundsResolver.cs"
).read_text(encoding="utf-8")
aim_source = (
    PROJECT / "decompiled-src/CompleteCheatMenu/Targeting/AimPoint.cs"
).read_text(encoding="utf-8")
assert "GetBoneTransform(HumanBodyBones.Head)" in bone_source
assert "GetBoneTransform(HumanBodyBones.Chest)" in bone_source
assert "GetBoneTransform(HumanBodyBones.Hips)" in bone_source
assert "SkinnedMeshRenderer" in bone_source
assert "bounds.extents.y * 0.5f" in bounds_source
assert "screen.z <= 0f" in aim_source

print("BONE_SELECTION_TESTS=PASS cases=7")
