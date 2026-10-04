import math
import sys
from pathlib import Path

TESTS = Path(__file__).resolve().parent
PROJECT = TESTS.parent
sys.path.insert(0, str(TESTS))

try:
    from cnkx_visible_aim_impl import apply_offsets, simulate_error
except (ImportError, ModuleNotFoundError):
    print("VISIBLE_AIM_TESTS=FAIL missing implementation")
    raise SystemExit(1)


# 指数平滑在 30/60/120 FPS 下经过同样时长应得到等效响应。
error_30 = simulate_error(90.0, response=8.0, fps=30, seconds=1.0, firing=True)
error_60 = simulate_error(90.0, response=8.0, fps=60, seconds=1.0, firing=True)
error_120 = simulate_error(90.0, response=8.0, fps=120, seconds=1.0, firing=True)
assert math.isclose(error_30, error_60, rel_tol=1e-9, abs_tol=1e-9)
assert math.isclose(error_60, error_120, rel_tol=1e-9, abs_tol=1e-9)
assert error_120 < 0.04

# 停止开火后完全停止相机修正。
assert simulate_error(45.0, response=8.0, fps=60, seconds=1.0, firing=False) == 45.0

# 距离下调与后坐补偿是两个独立设置。
base = apply_offsets(
    point=(0.0, 10.0, 100.0),
    distance=100.0,
    distance_drop=0.0,
    recoil=(2.0, 3.0),
    recoil_strength=0.0,
    zero_recoil=False,
)
assert base == {"point": (0.0, 10.0, 100.0), "pitch_comp": 0.0, "yaw_comp": 0.0}

dropped = apply_offsets(
    point=(0.0, 10.0, 100.0),
    distance=100.0,
    distance_drop=0.01,
    recoil=(2.0, 3.0),
    recoil_strength=0.0,
    zero_recoil=False,
)
assert dropped["point"] == (0.0, 9.0, 100.0)
assert dropped["pitch_comp"] == 0.0

compensated = apply_offsets(
    point=(0.0, 10.0, 100.0),
    distance=100.0,
    distance_drop=0.0,
    recoil=(2.0, 3.0),
    recoil_strength=0.5,
    zero_recoil=False,
)
assert compensated["point"] == (0.0, 10.0, 100.0)
assert compensated["pitch_comp"] == 1.5
assert compensated["yaw_comp"] == -1.0

# ZeroRecoil 开启时不再叠加第二套后坐补偿。
zero_recoil = apply_offsets(
    point=(0.0, 10.0, 100.0),
    distance=100.0,
    distance_drop=0.01,
    recoil=(2.0, 3.0),
    recoil_strength=1.0,
    zero_recoil=True,
)
assert zero_recoil["point"] == (0.0, 9.0, 100.0)
assert zero_recoil["pitch_comp"] == 0.0
assert zero_recoil["yaw_comp"] == 0.0

# 生产实现应读取真实开火字段和 PlayerCamera._rot，并使用指数平滑。
controller_source = (
    PROJECT / "decompiled-src/CompleteCheatMenu/Targeting/VisibleAimController.cs"
).read_text(encoding="utf-8")
weapon_source = (
    PROJECT / "decompiled-src/CompleteCheatMenu/Cheats/WeaponCheats.cs"
).read_text(encoding="utf-8")
tick_source = (
    PROJECT / "decompiled-src/CompleteCheatMenu/Runtime/TickDriver.cs"
).read_text(encoding="utf-8")
assert 'GameBinder.Field("Weapon", "_holdingFireInput")' in controller_source
assert 'GameBinder.Field("PlayerCamera", "_rot")' in controller_source
assert "1f - Mathf.Exp(-response * deltaTime)" in controller_source
assert "CheatState.ZeroRecoil" in controller_source
assert "VisibleAimController.Step" in weapon_source
assert "WeaponCheats.VisibleAimTick" in tick_source

print("VISIBLE_AIM_TESTS=PASS cases=7")
