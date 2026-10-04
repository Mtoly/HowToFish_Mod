import math
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent
sys.path.insert(0, str(ROOT))

try:
    from cnkx_math_impl import (
        inside_screen_radius,
        score_entity,
        select_best_bone,
        select_target,
    )
except (ImportError, ModuleNotFoundError):
    print("TARGETING_MATH_TESTS=FAIL missing implementation")
    raise SystemExit(1)


def close(actual, expected, tolerance=1e-6):
    assert math.isclose(actual, expected, rel_tol=tolerance, abs_tol=tolerance), (
        actual,
        expected,
    )


# 圆形屏幕半径：内部、边界、方形角落但圆外、明确圆外。
assert inside_screen_radius(3.0, 4.0, 5.0)
assert inside_screen_radius(0.0, 5.0, 5.0)
assert not inside_screen_radius(4.0, 4.0, 5.0)
assert not inside_screen_radius(5.01, 0.0, 5.0)

# 实体评分：距离、角度、屏幕距离与切换惩罚各自独立生效。
close(score_entity(20.0, 10.0, 8.0, False, 1.0, 0.5, 0.25, 8.0), 27.0)
close(score_entity(20.0, 10.0, 8.0, True, 1.0, 0.5, 0.25, 8.0), 35.0)
close(score_entity(20.0, 10.0, 8.0, False, 0.0, 1.0, 0.0, 8.0), 10.0)

# 骨骼评分只决定同一实体内的瞄准点，不污染实体级评分。
bones = [
    {"name": "head", "screen_distance": 22.0, "visible": True},
    {"name": "chest", "screen_distance": 8.0, "visible": True},
    {"name": "pelvis", "screen_distance": 3.0, "visible": False},
]
best_bone = select_best_bone(bones, require_visible=True)
assert best_bone["name"] == "chest"

targets = [
    {
        "name": "near-with-far-bone",
        "distance": 10.0,
        "angle": 2.0,
        "screen_distance": 20.0,
        "bone_score": 1000.0,
    },
    {
        "name": "far-with-near-bone",
        "distance": 30.0,
        "angle": 2.0,
        "screen_distance": 20.0,
        "bone_score": 1.0,
    },
]
selected = select_target(
    targets,
    current_name=None,
    distance_weight=1.0,
    angle_weight=0.5,
    screen_weight=0.0,
    switch_penalty=8.0,
)
assert selected["name"] == "near-with-far-bone"

print("TARGETING_MATH_TESTS=PASS cases=10")
