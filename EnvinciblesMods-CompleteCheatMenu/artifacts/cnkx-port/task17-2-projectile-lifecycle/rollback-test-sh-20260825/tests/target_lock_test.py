import sys
from pathlib import Path

TESTS = Path(__file__).resolve().parent
PROJECT = TESTS.parent
sys.path.insert(0, str(TESTS))

try:
    from cnkx_target_lock_impl import TargetingModel, VisibilityCacheModel
except (ImportError, ModuleNotFoundError):
    print("TARGET_LOCK_TESTS=FAIL missing implementation")
    raise SystemExit(1)


# 可见性按“对象 + 目标点”短周期缓存；同一点命中一次，不同点和过期后重新检测。
ray_calls = []


def raycast(entity, point):
    ray_calls.append((entity, point))
    return True


cache = VisibilityCacheModel(ttl=0.05)
assert cache.visible("fish", "head", 0.00, raycast)
assert cache.visible("fish", "head", 0.02, raycast)
assert len(ray_calls) == 1
assert cache.visible("fish", "chest", 0.02, raycast)
assert len(ray_calls) == 2
assert cache.visible("fish", "head", 0.06, raycast)
assert len(ray_calls) == 3

# 射线命中目标本体、子对象或父对象均视为可见。
assert cache.hit_matches("target", "target", {"target": None})
assert cache.hit_matches("target-child", "target", {"target-child": "target", "target": None})
assert cache.hit_matches("target", "target-child", {"target-child": "target", "target": None})
assert not cache.hit_matches("wall", "target", {"wall": None, "target": None})

# 同一次 acquire 同时确定实体与目标点，不读取上一帧瞄准点。
model = TargetingModel(lock_duration=0.20, switch_penalty=8.0)
first = model.acquire(
    [
        {"name": "near", "score": 10.0, "point": "head", "valid": True},
        {"name": "far", "score": 20.0, "point": "chest", "valid": True},
    ],
    now=1.0,
)
assert first == ("near", "head")

# 锁定期内当前目标仍有效时保持当前目标，即使另一目标基础分更低。
locked = model.acquire(
    [
        {"name": "near", "score": 10.0, "point": "pelvis", "valid": True},
        {"name": "far", "score": 1.0, "point": "head", "valid": True},
    ],
    now=1.10,
)
assert locked == ("near", "pelvis")

# 当前目标失效时立即释放，不等待保持时间结束。
released = model.acquire(
    [
        {"name": "near", "score": 0.0, "point": "head", "valid": False},
        {"name": "far", "score": 1.0, "point": "chest", "valid": True},
    ],
    now=1.11,
)
assert released == ("far", "chest")

# 保持期结束后按综合评分切换，并更新到本次 acquire 的目标点。
switched = model.acquire(
    [
        {"name": "far", "score": 30.0, "point": "pelvis", "valid": True},
        {"name": "third", "score": 2.0, "point": "head", "valid": True},
    ],
    now=1.40,
)
assert switched == ("third", "head")

# 生产源码必须集中 Acquire、缓存可见性，并从 TargetSolution 提供兼容视图。
system_source = (
    PROJECT / "decompiled-src/CompleteCheatMenu/Targeting/TargetingSystem.cs"
).read_text(encoding="utf-8")
visibility_source = (
    PROJECT / "decompiled-src/CompleteCheatMenu/Targeting/VisibilityCache.cs"
).read_text(encoding="utf-8")
weapon_source = (
    PROJECT / "decompiled-src/CompleteCheatMenu/Cheats/WeaponCheats.cs"
).read_text(encoding="utf-8")
assert "internal TargetSolution Acquire(Camera camera)" in system_source
assert "BoneResolver.Resolve" in system_source
assert "_visibility.IsVisible" in system_source
assert "hitTransform.IsChildOf(target)" in visibility_source
assert "target.IsChildOf(hitTransform)" in visibility_source
assert "internal static TargetSolution CurrentTarget" in weapon_source
assert "FindTarget(" not in weapon_source
assert "HasLineOfSight(" not in weapon_source

print("TARGET_LOCK_TESTS=PASS cases=8")
