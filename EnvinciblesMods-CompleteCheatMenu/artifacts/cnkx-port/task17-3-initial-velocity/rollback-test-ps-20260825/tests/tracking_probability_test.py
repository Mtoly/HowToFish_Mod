import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent
sys.path.insert(0, str(ROOT))

try:
    from cnkx_math_impl import should_track
except (ImportError, ModuleNotFoundError):
    print("TRACKING_PROBABILITY_TESTS=FAIL missing implementation")
    raise SystemExit(1)


samples = (0.0, 0.000001, 0.25, 0.5, 0.999999)

# 0% 必须始终关闭，包括随机样本恰好为 0。
assert all(not should_track(0.0, sample) for sample in samples)

# 100% 必须对 [0,1) 中所有样本开启。
assert all(should_track(100.0, sample) for sample in samples)

# 中间概率遵循 sample < probability / 100，而不是 0..100 共 101 个整数。
assert should_track(50.0, 0.499999)
assert not should_track(50.0, 0.5)

# 配置越界时钳制到 0..100。
assert not should_track(-10.0, 0.0)
assert should_track(150.0, 0.999999)

# 生产实现必须即时 Acquire，使用浮点 Random.value，并支持预测点与限速日志。
PROJECT = Path(__file__).resolve().parents[1]
redirector_source = (
    PROJECT / "decompiled-src/CompleteCheatMenu/Targeting/ShotRedirector.cs"
).read_text(encoding="utf-8")
patch_source = (
    PROJECT / "decompiled-src/CompleteCheatMenu/Patches/WeaponShoot_Patch.cs"
).read_text(encoding="utf-8")
weapon_source = (
    PROJECT / "decompiled-src/CompleteCheatMenu/Cheats/WeaponCheats.cs"
).read_text(encoding="utf-8")
assert "Random.value" in redirector_source
assert "return sample < probability" in redirector_source
assert "targetingSystem.Acquire(camera)" in redirector_source
assert "BallisticPredictor.Predict" in redirector_source
assert "_nextDiagnostic = Time.unscaledTime + 2f" in redirector_source
assert "WeaponCheats.PrepareShot()" in patch_source
assert "AlignFirePointToView" in weapon_source
assert "ShotRedirector.TryRedirect" in weapon_source

print("TRACKING_PROBABILITY_TESTS=PASS cases=6")
